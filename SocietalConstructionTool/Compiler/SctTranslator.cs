using Antlr4.Runtime.Misc;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Runtime;

namespace Sct.Compiler
{
    public class SctTranslator : SctBaseListener
    {
        public NamespaceDeclarationSyntax? Root { get; private set; }
        private readonly StackAdapter<CSharpSyntaxNode> _stack = new();
        public override void ExitStart(SctParser.StartContext context)
        {
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("MyNamespace"));
            var @class = SyntaxFactory.ClassDeclaration("GlobalClass")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            var members = _stack.ToArray<MemberDeclarationSyntax>();

            @class = @class.AddMembers(members);
            Root = @namespace.AddMembers(@class);
        }

        public override void EnterClass_def([NotNull] SctParser.Class_defContext context)
        {
            var @class = SyntaxFactory.ClassDeclaration("tmp")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBaseList(SyntaxFactory.BaseList(
                    SyntaxFactory.SeparatedList(
                        (IEnumerable<BaseTypeSyntax>)[SyntaxFactory.SimpleBaseType(
                            SyntaxFactory.QualifiedName(
                                // If namespace = null this could produce invalid code
                                SyntaxFactory.IdentifierName(typeof(BaseAgent).Namespace ?? ""),
                                SyntaxFactory.IdentifierName(typeof(BaseAgent).Name)
                                )
                            )]
                        )
                    )
                );
            _stack.Push(@class);
        }

        public override void ExitClass_def([NotNull] SctParser.Class_defContext context)
        {
            // Pop functions, decorators, and states, but throw away parameter list, as it is not relevant post-type checking
            // All constructors are equal, so we can just create a custom one
            var members = _stack.PopUntil<ParameterListSyntax, MemberDeclarationSyntax>(out var _);
            var @class = _stack.Pop<ClassDeclarationSyntax>();
            var idText = context.ID().GetText();
            var constructor = CreateConstructor(idText);
            var cloneMethod = CreateCloneMethod(idText);

            @class = @class.WithIdentifier(SyntaxFactory.Identifier(idText))
                .AddMembers(constructor)
                .AddMembers(members)
                .AddMembers(cloneMethod);

            _stack.Push(@class);
        }

        private MethodDeclarationSyntax CreateCloneMethod(string className)
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(typeof(BaseAgent).Name),
                "Clone"
            )
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.ReturnStatement(
                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(className))
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName("State")),
                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName("Fields"))
                            })
                        )
                    )
                )
            ));
        }

        private ConstructorDeclarationSyntax CreateConstructor(string className)
        {
            var parameters = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(new[]
                {
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("state"))
                        .WithType(SyntaxFactory.ParseTypeName("string")),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("fields"))
                        .WithType(SyntaxFactory.ParseTypeName("IDictionary<string, dynamic>"))
                })
            );

            return SyntaxFactory.ConstructorDeclaration(className)
                .WithParameterList(parameters) // Add parameters
                .WithBody(SyntaxFactory.Block()) // Add empty body
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)) // Make public
                .WithInitializer(SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, // Add base constructor call
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("state")),
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("fields"))
                        })
                    )
                ));
        }

        public override void ExitArgs_def([NotNull] SctParser.Args_defContext context)
        {
            // create list of parameters by zipping ID and type
            var @params = context.ID().Zip(context.type(), (id, type) =>
                SyntaxFactory.Parameter(SyntaxFactory.Identifier(id.GetText())) // set name
                    .WithType(SyntaxFactory.ParseTypeName(type.GetText())) // set type
            );

            // add to parameter list
            _stack.Push(SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(@params)
            ));
        }

        public override void EnterArgs_call([NotNull] SctParser.Args_callContext context)
        {
            _stack.PushMarker();
        }

        public override void ExitArgs_call(SctParser.Args_callContext context)
        {
            var args = _stack.PopUntilMarker<ExpressionSyntax>();
            _stack.Push(SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(args.Select(SyntaxFactory.Argument))
            ));
        }

        public override void ExitDecorator([NotNull] SctParser.DecoratorContext context)
        {
            var childBlock = _stack.Pop<BlockSyntax>();
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                context.ID().GetText()
            );
            method = method.WithBody(childBlock);
            _stack.Push(method);
        }

        public override void ExitFunction([NotNull] SctParser.FunctionContext context)
        {
            var childBlock = _stack.Pop<BlockSyntax>();
            var @params = _stack.Pop<ParameterListSyntax>();
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), // change to actual type
                context.ID().GetText()
            )
            .WithParameterList(@params)
            .WithBody(childBlock);
            _stack.Push(method);
        }

        public override void ExitState_decorator([NotNull] SctParser.State_decoratorContext context)
        {
            // push all decorators to the stack as method calls
            var state = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(context.ID().GetText())
            );

            _stack.Push(state);
        }

        public override void ExitState([NotNull] SctParser.StateContext context)
        {
            var stateLogic = _stack.Pop<BlockSyntax>();
            var decorators = _stack.PopWhile<InvocationExpressionSyntax>()
                .Select(SyntaxFactory.ExpressionStatement)
                .Cast<StatementSyntax>();

            // create new body with decorators first then state logic
            var body = SyntaxFactory.Block()
                .AddStatements(decorators.ToArray())
                .AddStatements(stateLogic.Statements.ToArray());

            // TODO: Add ctx as method argument
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                context.ID().GetText()
            );

            _stack.Push(method.WithBody(body));
        }

        public override void EnterStatement_list([NotNull] SctParser.Statement_listContext context)
        {
            var block = SyntaxFactory.Block();
            _stack.Push(block);
        }

        public override void ExitStatement_list([NotNull] SctParser.Statement_listContext context)
        {
            var statements = _stack.PopUntil<BlockSyntax, StatementSyntax>(out var parentBlock);
            parentBlock = parentBlock.AddStatements(statements);
            _stack.Push(parentBlock);
        }

        public override void ExitWhile([NotNull] SctParser.WhileContext context)
        {
            // TODO: Expand with expression from stack
            var childBlock = _stack.Pop<BlockSyntax>();
            var @while = SyntaxFactory.WhileStatement(
                SyntaxFactory.ParseExpression("true"),
                childBlock
            );
            _stack.Push(@while);
        }

        public override void ExitLiteralExpression([NotNull] SctParser.LiteralExpressionContext context)
        {
            var contxt = context.LIT();
            var litExp = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(contxt.GetText()));
            _stack.Push(litExp);
        }

        public override void ExitIDExpression([NotNull] SctParser.IDExpressionContext context)
        {
            var contxt = context.ID();
            var idExp = SyntaxFactory.IdentifierName(contxt.GetText());
            _stack.Push(idExp);
        }

        public override void ExitBooleanExpression([NotNull] SctParser.BooleanExpressionContext context)
        {
            var exp2 = (ExpressionSyntax)_stack.Pop();
            var exp1 = (ExpressionSyntax)_stack.Pop();

            var @operator = SyntaxKind.None;
            if (context.op == context.GT()?.Symbol)
            {
                @operator = SyntaxKind.GreaterThanExpression;
            }
            else if (context.op == context.LT()?.Symbol)
            {
                @operator = SyntaxKind.LessThanExpression;
            }
            else if (context.op == context.GTE()?.Symbol)
            {
                @operator = SyntaxKind.GreaterThanOrEqualExpression;
            }
            else if (context.op == context.LTE()?.Symbol)
            {
                @operator = SyntaxKind.LessThanOrEqualExpression;
            }
            else if (context.op == context.EQ()?.Symbol)
            {
                @operator = SyntaxKind.EqualsExpression;
            }
            else if (context.op == context.NEQ()?.Symbol)
            {
                @operator = SyntaxKind.NotEqualsExpression;
            }
            else if (context.op == context.AND()?.Symbol)
            {
                @operator = SyntaxKind.LogicalAndExpression;
            }
            else if (context.op == context.OR()?.Symbol)
            {
                @operator = SyntaxKind.LogicalOrExpression;
            }

            var trueValue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1));
            var falseValue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));

            var condition = SyntaxFactory.BinaryExpression(@operator, exp1, exp2);
            _stack.Push(SyntaxFactory.ConditionalExpression(condition, trueValue, falseValue));
        }

        public override void ExitParenthesisExpression([NotNull] SctParser.ParenthesisExpressionContext context)
        {
            var node = _stack.Pop<ExpressionSyntax>();
            _stack.Push(SyntaxFactory.ParenthesizedExpression(node));
        }

        public override void ExitTypecastExpression([NotNull] SctParser.TypecastExpressionContext context)
        {
            var node = _stack.Pop<ExpressionSyntax>();
            var type = SyntaxFactory.ParseTypeName(context.type().GetText());
            _stack.Push(SyntaxFactory.CastExpression(type, node));
        }

        public override void ExitLogicalNotExpression([NotNull] SctParser.LogicalNotExpressionContext context)
        {
            var expression = _stack.Pop<ExpressionSyntax>();
            var @operator = SyntaxKind.NotEqualsExpression;
            var falseValue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));
            var trueValue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1));
            var condition = SyntaxFactory.BinaryExpression(@operator, expression, falseValue);
            _stack.Push(SyntaxFactory.ConditionalExpression(condition, falseValue, trueValue));
        }


        public override void ExitUnaryMinusExpression([NotNull] SctParser.UnaryMinusExpressionContext context)
        {
            var expression = _stack.Pop<ExpressionSyntax>();
            _stack.Push(SyntaxFactory.PrefixUnaryExpression(
                SyntaxKind.UnaryMinusExpression,
                expression
            ));
        }

        public override void ExitCallExpression([NotNull] SctParser.CallExpressionContext context)
        {
            var args = _stack.Pop<ArgumentListSyntax>();
            var id = SyntaxFactory.IdentifierName(context.ID().GetText());
            var call = SyntaxFactory.InvocationExpression(id, args);
            _stack.Push(call);
        }

        /// <summary>
        /// Pops items from the stack until it finds an element of type TParent
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>


        // BELOW ARE TEMPORARY METHODS TO MAKE THE COMPILER WORK
        // WE NEED TO DROP BLOCKS FROM THE STACK UNTILL THEY ARE PROPERLY IMPLEMENTED
        public override void ExitBreak([NotNull] SctParser.BreakContext context)
        {
            //todo check if we can break before doing it
            _stack.Push(SyntaxFactory.BreakStatement());
        }

        public override void ExitContinue([NotNull] SctParser.ContinueContext context)
        {
            //todo check if we can continue before doing it
            _stack.Push(SyntaxFactory.ContinueStatement());
        }

        public override void ExitIf([NotNull] SctParser.IfContext context)
        {
            var @if = IfHelper();
            _stack.Push(@if);
        }
        public override void ExitElseif([NotNull] SctParser.ElseifContext context)
        {
            // Else if is not its own construct. Wrap @if in an else clause
            var @if = IfHelper();
            var @else = SyntaxFactory.ElseClause(@if);
            _stack.Push(@else);
        }
        public override void ExitElse([NotNull] SctParser.ElseContext context)
        {
            var childBlock = _stack.Pop<BlockSyntax>();
            var @else = SyntaxFactory.ElseClause(childBlock);
            _stack.Push(@else);
        }

        private IfStatementSyntax IfHelper()
        {
            // Stack order is either <block> <exp> or <else> <block> <exp>
            var @else = _stack.Peek() is ElseClauseSyntax ? _stack.Pop<ElseClauseSyntax>() : null;
            var block = _stack.Pop<BlockSyntax>();
            var expression = _stack.Pop<ExpressionSyntax>();

            // Add else if it exists
            var @if = SyntaxFactory.IfStatement(expression, block);
            return @else == null ? @if : @if.WithElse(@else);
        }

        public override void ExitExit([NotNull] SctParser.ExitContext context){
            _stack.Push(SyntaxFactory.ParseStatement("ctx.Exit();"));
        }

    }
}

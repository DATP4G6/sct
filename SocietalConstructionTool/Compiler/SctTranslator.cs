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
            var members = _stack.PopUntil<ParameterListSyntax, MemberDeclarationSyntax>(out var _);
            var @class = _stack.Pop<ClassDeclarationSyntax>();
            var constructor = CreateConstructor(context.ID().GetText());

            @class = @class.WithIdentifier(SyntaxFactory.Identifier(context.ID().GetText()))
                .AddMembers(constructor)
                .AddMembers(members);

            _stack.Push(@class);
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

        public override void ExitState([NotNull] SctParser.StateContext context)
        {
            // TODO: Expand with preprending decorator calls
            var childBlock = _stack.Pop<BlockSyntax>();
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                context.ID().GetText()
            );
            method = method.WithBody(childBlock);
            _stack.Push(method);
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

            var expOperator = SyntaxKind.None;
            if (context.GT() != null)
            {
                expOperator = SyntaxKind.GreaterThanExpression;
            }
            else if (context.LT() != null)
            {
                expOperator = SyntaxKind.GreaterThanExpression;
            }
            else if (context.GTE() != null)
            {
                expOperator = SyntaxKind.GreaterThanOrEqualExpression;
            }
            else if (context.LTE() != null)
            {
                expOperator = SyntaxKind.LessThanOrEqualExpression;
            }
            else if (context.EQ() != null)
            {
                expOperator = SyntaxKind.EqualsExpression;
            }
            else if (context.NEQ() != null)
            {
                expOperator = SyntaxKind.NotEqualsExpression;
            }
            else if (context.AND() != null)
            {
                expOperator = SyntaxKind.LogicalAndExpression;
            }
            else if (context.OR() != null)
            {
                expOperator = SyntaxKind.LogicalOrExpression;
            }

            var trueValue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1));
            var falseValue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));

            var condition = SyntaxFactory.BinaryExpression(expOperator, exp1, exp2);
            _stack.Push(SyntaxFactory.ConditionalExpression(condition, trueValue, falseValue));
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
        public override void ExitIf([NotNull] SctParser.IfContext context)
        {
            _ = _stack.Pop();
        }
        public override void ExitElseif([NotNull] SctParser.ElseifContext context)
        {
            _ = _stack.Pop();
        }
        public override void ExitElse([NotNull] SctParser.ElseContext context)
        {
            _ = _stack.Pop();
        }
    }
}

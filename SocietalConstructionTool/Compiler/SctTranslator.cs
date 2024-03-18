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
            var members = _stack.PopUntil<ClassDeclarationSyntax, MemberDeclarationSyntax>(out var @class);
            @class = @class.WithIdentifier(SyntaxFactory.Identifier(context.ID().GetText()))
                .AddMembers(members);
            _stack.Push(@class);
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
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                context.ID().GetText()
            );
            method = method.WithBody(childBlock);
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
            var childBlockNode = _stack.Pop();
            //var expressionNode = _stack.Pop();
            var expressionNode = SyntaxFactory.ParseExpression("true");

            if (expressionNode is ExpressionSyntax expression)
            {
                if (childBlockNode is BlockSyntax childBlock)
                {
                    var @if = SyntaxFactory.IfStatement(expression, childBlock);
                    _stack.Push(@if);
                }
                else if (childBlockNode is ElseClauseSyntax @else)
                {
                    var blockNode = _stack.Pop();
                    if (blockNode is BlockSyntax block)
                    {
                        var @if = SyntaxFactory.IfStatement(expression, block, @else);
                        _stack.Push(@if);
                    }

                }
            }
            else
            {
                throw new Exception("Node was of an unrecognized type");
            }


        }
        public override void ExitElseif([NotNull] SctParser.ElseifContext context)
        {
            var childBlockNode = _stack.Pop();
            var expressionNode = SyntaxFactory.ParseExpression("true");
            // var expressionNode = _stack.Pop();

            if (expressionNode is ExpressionSyntax expression)
            {
                if (childBlockNode is BlockSyntax childBlock)
                {
                    var @if = SyntaxFactory.IfStatement(expression, childBlock);
                    var @else = SyntaxFactory.ElseClause(@if);
                    _stack.Push(@else);
                }
                else if (childBlockNode is ElseClauseSyntax @else)
                {
                    var blockNode = _stack.Pop();
                    if (blockNode is BlockSyntax block)
                    {
                        var @if = SyntaxFactory.IfStatement(expression, block, @else);
                        var @else2 = SyntaxFactory.ElseClause(@if);
                        _stack.Push(@else2);
                    }

                }
                else
                {
                    throw new Exception("Node was of an unrecognized type");
                }
            }
        }
        public override void ExitElse([NotNull] SctParser.ElseContext context)
        {
            var childBlockNode = _stack.Pop();

            if (childBlockNode is BlockSyntax childBlock)
            {
                var @else = SyntaxFactory.ElseClause(childBlock);
                _stack.Push(@else);
            }
            else
            {
                throw new Exception("Node was of an unrecognized type");
            }
        }
    }
}

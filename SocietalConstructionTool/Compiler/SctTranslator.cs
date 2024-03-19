using Antlr4.Runtime.Misc;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Compiler.Exceptions;
using Sct.Runtime;

namespace Sct.Compiler
{
    public class SctTranslator : SctBaseListener
    {
        public NamespaceDeclarationSyntax? Root { get; private set; }
        private readonly Stack<CSharpSyntaxNode> _stack = new();
        public override void ExitStart(SctParser.StartContext context)
        {
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("MyNamespace"));
            var @class = SyntaxFactory.ClassDeclaration("GlobalClass")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            var children = _stack.ToArray();
            var members = children.OfType<MemberDeclarationSyntax>().ToArray();
            if (members.Length != children.Length)
            {
                throw new UnrecognizedNodeException("Not all children were of type MemberDeclarationSyntax");
            }

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
            var members = PopUntil<ClassDeclarationSyntax, MemberDeclarationSyntax>(out var @class);
            @class = @class.WithIdentifier(SyntaxFactory.Identifier(context.ID().GetText()))
                .AddMembers(members);
            _stack.Push(@class);
        }

        public override void ExitDecorator([NotNull] SctParser.DecoratorContext context)
        {
            var node = _stack.Pop();
            if (node is BlockSyntax childBlock)
            {
                var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    context.ID().GetText()
                );
                method = method.WithBody(childBlock);
                _stack.Push(method);
            }
            else
            {
                throw new UnrecognizedNodeException(nameof(BlockSyntax), node.GetType().Name);
            }
        }

        public override void ExitFunction([NotNull] SctParser.FunctionContext context)
        {
            var node = _stack.Pop();
            if (node is BlockSyntax childBlock)
            {
                var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    context.ID().GetText()
                );
                method = method.WithBody(childBlock);
                _stack.Push(method);
            }
            else
            {
                throw new UnrecognizedNodeException(nameof(BlockSyntax), node.GetType().Name);
            }
        }

        public override void ExitState([NotNull] SctParser.StateContext context)
        {
            // TODO: Expand with preprending decorator calls
            var node = _stack.Pop();
            if (node is BlockSyntax childBlock)
            {
                var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    context.ID().GetText()
                );
                method = method.WithBody(childBlock);
                _stack.Push(method);
            }
            else
            {
                throw new UnrecognizedNodeException(nameof(BlockSyntax), node.GetType().Name);
            }
        }

        public override void EnterStatement_list([NotNull] SctParser.Statement_listContext context)
        {
            var block = SyntaxFactory.Block();
            _stack.Push(block);
        }

        public override void ExitStatement_list([NotNull] SctParser.Statement_listContext context)
        {
            var statements = PopUntil<BlockSyntax, StatementSyntax>(out var parentBlock);
            parentBlock = parentBlock.AddStatements(statements);
            _stack.Push(parentBlock);
        }

        public override void ExitWhile([NotNull] SctParser.WhileContext context)
        {
            // TODO: Expand with expression from stack
            var node = _stack.Pop();
            if (node is BlockSyntax childBlock)
            {
                var @while = SyntaxFactory.WhileStatement(
                    SyntaxFactory.ParseExpression("true"),
                    childBlock
                );
                _stack.Push(@while);
            }
            else
            {
                throw new UnrecognizedNodeException(nameof(BlockSyntax), node.GetType().Name);
            }
        }

        /// <summary>
        /// Pops items from the stack until it finds an element of type TParent
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private TItem[] PopUntil<TParent, TItem>(out TParent parent)
            where TParent : CSharpSyntaxNode
            where TItem : CSharpSyntaxNode
        {
            List<TItem> items = [];
            while (_stack.Peek() is TItem item and not TParent)
            {
                items.Add(item);
                _ = _stack.Pop();
            }

            var node = _stack.Pop();
            if (node is TParent parentNode)
            {
                parent = parentNode;
            }
            else
            {
                // throw new Exception("Node was of an unrecognized type");
                throw new UnrecognizedNodeException(nameof(TParent), node.GetType().Name);
            }

            // Popping stack reversed order of items
            items.Reverse();
            return items.ToArray();
        }

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

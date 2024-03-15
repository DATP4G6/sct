using Antlr4.Runtime.Misc;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sct
{
    public class SctListener : SctBaseListener
    {
        public NamespaceDeclarationSyntax? Root { get; private set; }
        private readonly Stack<CSharpSyntaxNode> _stack = new();
        public override void EnterStart(SctParser.StartContext context)
        {
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("MyNamespace"));
            _stack.Push(@namespace);
        }

        public override void ExitStart(SctParser.StartContext context)
        {
            var namespaceNode = _stack.Pop();

            Root = namespaceNode is NamespaceDeclarationSyntax @namespace ? @namespace : throw new Exception("Node was of an unrecognized type");
        }

        public override void EnterClass_def([NotNull] SctParser.Class_defContext context)
        {
            var @class = SyntaxFactory.ClassDeclaration("tmp")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            _stack.Push(@class);
        }

        public override void ExitClass_def([NotNull] SctParser.Class_defContext context)
        {
            var classNode = _stack.Pop();
            var namespaceNode = _stack.Pop();

            if (classNode is ClassDeclarationSyntax @class && namespaceNode is NamespaceDeclarationSyntax @namespace)
            {
                @class = @class.WithIdentifier(SyntaxFactory.Identifier(
                    context.ID().GetText()
                ));
                @namespace = @namespace.AddMembers(@class);
                _stack.Push(@namespace);
            }
            else
            {
                throw new Exception("Node was of an unrecognized type");
            }
        }

        public override void EnterDecorator([NotNull] SctParser.DecoratorContext context)
        {
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "tmp"
            );
            method = method.AddBodyStatements([]);
            method = method.AddBodyStatements(SyntaxFactory.ParseStatement("throw new NotImplementedException();"));
            _stack.Push(method);
        }

        public override void ExitDecorator([NotNull] SctParser.DecoratorContext context)
        {
            var methodNode = _stack.Pop();
            var classNode = _stack.Pop();

            if (methodNode is MethodDeclarationSyntax method && classNode is ClassDeclarationSyntax @class)
            {
                method = method.WithIdentifier(SyntaxFactory.Identifier(
                    context.ID().GetText()
                ));
                @class = @class.AddMembers(method);
                _stack.Push(@class);
            }
            else
            {
                throw new Exception("Node was of an unrecognized type");
            }
        }

        public override void EnterState([NotNull] SctParser.StateContext context)
        {
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "tmp"
            );
            method = method.AddBodyStatements([]);
            method = method.AddBodyStatements(SyntaxFactory.ParseStatement("throw new NotImplementedException();"));
            _stack.Push(method);
        }

        public override void ExitState([NotNull] SctParser.StateContext context)
        {
            var methodNode = _stack.Pop();
            var classNode = _stack.Pop();

            if (methodNode is MethodDeclarationSyntax method && classNode is ClassDeclarationSyntax @class)
            {
                method = method.WithIdentifier(SyntaxFactory.Identifier(
                    context.ID().GetText()
                ));
                @class = @class.AddMembers(method);
                _stack.Push(@class);
            }
            else
            {
                throw new Exception("Node was of an unrecognized type");
            }
        }

        public override void EnterWhile([NotNull] SctParser.WhileContext context)
        {
            var @while = SyntaxFactory.WhileStatement(
                SyntaxFactory.ParseExpression("true"),
                SyntaxFactory.ParseStatement("throw new NotImplementedException();")
            );

            _stack.Push(@while);
        }

        public override void ExitWhile([NotNull] SctParser.WhileContext context)
        {
            var whileNode = _stack.Pop();
            var parentNode = _stack.Pop();

            if (whileNode is WhileStatementSyntax @while)
            {
                switch (parentNode)
                {
                    case MethodDeclarationSyntax method:
                        method = method.AddBodyStatements(@while);
                        _stack.Push(method);
                        break;
                    case WhileStatementSyntax parentWhile:
                        parentWhile = parentWhile.WithStatement(@while);
                        _stack.Push(parentWhile);
                        break;
                    case IfStatementSyntax parentIf:
                        parentIf = parentIf.WithStatement(@while);
                        _stack.Push(parentIf);
                        break;
                    default:
                        throw new Exception("While-loop defined outside valid scope");
                }
            }
            else
            {
                throw new Exception("Node was of an unrecognized type");
            }
        }
    }
}

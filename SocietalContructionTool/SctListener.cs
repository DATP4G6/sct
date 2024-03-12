using Antlr4.Runtime.Misc;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sct
{
    public class SctListener : SctBaseListener
    {
        public NamespaceDeclarationSyntax Namespace { get; private set; }
        private ClassDeclarationSyntax _globalClass;
        private ClassDeclarationSyntax _currentClass;
        private Stack<MethodDeclarationSyntax> _methods = new Stack<MethodDeclarationSyntax>();
        public override void EnterStart(SctParser.StartContext context)
        {
            Console.WriteLine("EnterStart");
            Namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("MyNamespace"));
        }

        public override void ExitStart(SctParser.StartContext context)
        {
            Console.WriteLine("ExitStart");
        }

        public override void EnterClass_def([NotNull] SctParser.Class_defContext context)
        {
            _currentClass = SyntaxFactory.ClassDeclaration("tmp")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }

        public override void ExitClass_def([NotNull] SctParser.Class_defContext context)
        {
            _currentClass = _currentClass.WithIdentifier(SyntaxFactory.Identifier(
                context.ID().GetText()
            ));
            Namespace = Namespace.AddMembers(_currentClass);
        }

        public override void EnterState([NotNull] SctParser.StateContext context)
        {
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "tmp"
            );
            method = method.AddBodyStatements([]);
            method = method.AddBodyStatements(SyntaxFactory.ParseStatement("throw new NotImplementedException();"));
            _methods.Push(method);
        }

        public override void ExitState([NotNull] SctParser.StateContext context)
        {
            var method = _methods.Pop();
            _currentClass = _currentClass.AddMembers(method.WithIdentifier(SyntaxFactory.Identifier(
                context.ID().GetText()
            )));
        }
    }
}

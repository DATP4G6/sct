namespace Sct.Compiler.Syntax
{
    public class AstFilenameVisitor(string filename) : SctBaseBuilderSyntaxVisitor
    {
        private string Filename { get; } = filename;

        public override SctSyntax Visit(SctSyntax node) {
            var newNode = base.Visit(node);
            newNode.Context.Filename = Filename;
            return newNode;
        }
    }
}

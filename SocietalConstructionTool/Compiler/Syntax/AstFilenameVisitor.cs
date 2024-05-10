namespace Sct.Compiler.Syntax
{
    public class AstFilenameVisitor : SctBaseSyntaxVisitor<SctSyntax>
    {
        private string Filename { get; }

        public AstFilenameVisitor(string filename)
        {
            Filename = filename;
        }

        public override SctSyntax Visit(SctSyntax node) {
            Console.WriteLine(node.GetType().Name);
            node.Context.AddFilename(Filename);
            return VisitChildren(node);
            // return node.Children.Select(x => x.Accept(this)).FirstOrDefault() ?? node;
        }
    }
}

namespace Sct.Compiler.Syntax
{
    public class SctEnterStatementSyntax(SctSyntaxContext context, string id) : SctStatementSyntax(context)
    {
        public string Id => id;
        public override IEnumerable<SctSyntax> Children => [];
    }
}

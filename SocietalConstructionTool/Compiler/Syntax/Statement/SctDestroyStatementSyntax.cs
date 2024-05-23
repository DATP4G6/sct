namespace Sct.Compiler.Syntax
{
    public class SctDestroyStatementSyntax(SctSyntaxContext context) : SctStatementSyntax(context)
    {
        public override IEnumerable<SctSyntax> Children => [];
    }
}

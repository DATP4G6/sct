namespace Sct.Compiler.Syntax
{
    public class SctBreakStatementSyntax(SctSyntaxContext context) : SctStatementSyntax(context)
    {
        public override IEnumerable<SctSyntax> Children => [];
    }
}

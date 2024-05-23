namespace Sct.Compiler.Syntax
{
    public class SctContinueStatementSyntax(SctSyntaxContext context) : SctStatementSyntax(context)
    {
        public override IEnumerable<SctSyntax> Children => [];
    }
}

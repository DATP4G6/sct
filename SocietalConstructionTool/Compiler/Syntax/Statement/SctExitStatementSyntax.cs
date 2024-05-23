namespace Sct.Compiler.Syntax
{
    public class SctExitStatementSyntax(SctSyntaxContext context) : SctStatementSyntax(context)
    {
        public override IEnumerable<SctSyntax> Children => [];
    }
}

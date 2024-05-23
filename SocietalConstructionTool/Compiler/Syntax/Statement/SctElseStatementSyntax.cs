namespace Sct.Compiler.Syntax
{
    public class SctElseStatementSyntax(SctSyntaxContext context, SctBlockStatementSyntax block) : SctStatementSyntax(context)
    {
        public SctBlockStatementSyntax Block => block;
        public override IEnumerable<SctSyntax> Children => [Block];
    }
}

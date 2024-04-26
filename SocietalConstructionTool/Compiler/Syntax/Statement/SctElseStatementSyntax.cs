namespace Sct.Compiler.Syntax
{
    public class SctElseStatementSyntax(SctBlockStatementSyntax block) : SctStatementSyntax
    {
        public SctBlockStatementSyntax Block => block;
    }
}

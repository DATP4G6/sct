namespace Sct.Compiler.Syntax
{
    public class SctWhileStatementSyntax(SctExpressionSyntax expression, SctBlockStatementSyntax block) : SctStatementSyntax
    {
        public SctExpressionSyntax Expression => expression;
        public SctBlockStatementSyntax Block => block;
    }
}

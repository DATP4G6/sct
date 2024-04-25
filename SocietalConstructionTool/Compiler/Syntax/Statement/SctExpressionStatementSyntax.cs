namespace Sct.Compiler.Syntax
{
    public class SctExpressionStatementSyntax(SctExpressionSyntax expression) : SctStatementSyntax
    {
        public SctExpressionSyntax Expression => expression;
    }
}

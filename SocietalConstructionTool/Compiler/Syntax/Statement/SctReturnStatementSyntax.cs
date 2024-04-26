namespace Sct.Compiler.Syntax
{
    public class SctReturnStatementSyntax(SctExpressionSyntax? expression) : SctStatementSyntax
    {
        public SctExpressionSyntax? Expression => expression;
    }
}

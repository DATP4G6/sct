namespace Sct.Compiler.Syntax
{
    public class SctNotExpressionSyntax(SctExpressionSyntax expression) : SctExpressionSyntax
    {
        public SctExpressionSyntax Expression => expression;
    }
}

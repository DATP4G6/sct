namespace Sct.Compiler.Syntax
{
    public class SctUnaryMinusExpressionSyntax(SctExpressionSyntax expression) : SctExpressionSyntax
    {
        public SctExpressionSyntax Expression => expression;
    }
}

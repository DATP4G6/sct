namespace Sct.Compiler.Syntax
{
    public enum SctBinaryOperator
    {
        Mult,
        Div,
        Plus,
        Minus,
        Mod
    }

    public class SctBinaryExpressionSyntax(SctExpressionSyntax left, SctExpressionSyntax right, SctBinaryOperator op) : SctExpressionSyntax
    {
        public SctExpressionSyntax Left => left;
        public SctExpressionSyntax Right => right;
        public SctBinaryOperator Op => op;
    }
}

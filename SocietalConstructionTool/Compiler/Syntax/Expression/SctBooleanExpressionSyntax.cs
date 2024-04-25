namespace Sct.Compiler.Syntax
{
    public enum SctBooleanOperator
    {
        And,
        Or,
        Eq,
        Neq,
        Gt,
        Lt,
        Gte,
        Lte,
        Not
    }

    public class SctBooleanExpressionSyntax(SctExpressionSyntax left, SctExpressionSyntax right, SctBooleanOperator op) : SctExpressionSyntax
    {
        public SctExpressionSyntax Left => left;
        public SctExpressionSyntax Right => right;
        public SctBooleanOperator Op => op;
    }
}

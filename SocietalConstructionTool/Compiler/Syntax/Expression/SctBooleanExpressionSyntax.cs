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

    public class SctBooleanExpressionSyntax(SctSyntaxContext context, SctExpressionSyntax left, SctExpressionSyntax right, SctBooleanOperator op) : SctExpressionSyntax(context)
    {
        public SctExpressionSyntax Left => left;
        public SctExpressionSyntax Right => right;
        public SctBooleanOperator Op => op;
        public override IEnumerable<SctSyntax> Children => [Left, Right];
    }
}

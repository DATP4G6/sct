using Antlr4.Runtime;

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

    public class SctBinaryExpressionSyntax(ParserRuleContext context, SctExpressionSyntax left, SctExpressionSyntax right, SctBinaryOperator op) : SctExpressionSyntax(context)
    {
        public SctExpressionSyntax Left => left;
        public SctExpressionSyntax Right => right;
        public SctBinaryOperator Op => op;
        public override IEnumerable<SctSyntax> Children => [Left, Right];
    }
}

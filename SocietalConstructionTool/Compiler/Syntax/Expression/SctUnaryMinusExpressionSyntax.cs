namespace Sct.Compiler.Syntax
{
    public class SctUnaryMinusExpressionSyntax(SctSyntaxContext context, SctExpressionSyntax expression) : SctExpressionSyntax(context)
    {
        public SctExpressionSyntax Expression => expression;
        public override IEnumerable<SctSyntax> Children => [Expression];
    }
}

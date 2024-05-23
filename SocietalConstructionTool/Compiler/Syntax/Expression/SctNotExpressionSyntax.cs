namespace Sct.Compiler.Syntax
{
    public class SctNotExpressionSyntax(SctSyntaxContext context, SctExpressionSyntax expression) : SctExpressionSyntax(context)
    {
        public SctExpressionSyntax Expression => expression;
        public override IEnumerable<SctSyntax> Children => [Expression];
    }
}

namespace Sct.Compiler.Syntax
{
    public class SctParenthesisExpressionSyntax(SctSyntaxContext context, SctExpressionSyntax expression) : SctExpressionSyntax(context)
    {
        public SctExpressionSyntax Expression => expression;
        public override IEnumerable<SctSyntax> Children => [Expression];
    }
}

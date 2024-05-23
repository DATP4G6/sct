namespace Sct.Compiler.Syntax
{
    public class SctTypecastExpressionSyntax(SctSyntaxContext context, SctTypeSyntax type, SctExpressionSyntax expression) : SctExpressionSyntax(context)
    {
        public SctTypeSyntax Type => type;
        public SctExpressionSyntax Expression => expression;
        public override IEnumerable<SctSyntax> Children => [Type, Expression];
    }
}

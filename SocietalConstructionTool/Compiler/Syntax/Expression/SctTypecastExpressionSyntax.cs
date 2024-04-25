namespace Sct.Compiler.Syntax
{
    public class SctTypecastExpressionSyntax(SctTypeSyntax type, SctExpressionSyntax expression) : SctExpressionSyntax
    {
        public SctTypeSyntax Type => type;
        public SctExpressionSyntax Expression => expression;
    }
}

namespace Sct.Compiler.Syntax
{
    public class SctParenthesisExpressionSyntax(SctExpressionSyntax expression) : SctExpressionSyntax
    {
        public SctExpressionSyntax Expression => expression;
    }
}

namespace Sct.Compiler.Syntax
{
    public class SctExpressionStatementSyntax(SctSyntaxContext context, SctExpressionSyntax expression) : SctStatementSyntax(context)
    {
        public SctExpressionSyntax Expression => expression;
        public override IEnumerable<SctSyntax> Children => [Expression];
    }
}

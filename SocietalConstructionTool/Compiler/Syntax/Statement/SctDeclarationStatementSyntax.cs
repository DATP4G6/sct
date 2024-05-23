namespace Sct.Compiler.Syntax
{
    public class SctDeclarationStatementSyntax(
        SctSyntaxContext context,
        SctTypeSyntax type,
        string id,
        SctExpressionSyntax expression
    ) : SctStatementSyntax(context)
    {
        public SctTypeSyntax Type => type;
        public string Id => id;
        public SctExpressionSyntax Expression => expression;
        public override IEnumerable<SctSyntax> Children => [Type, Expression];
    }
}

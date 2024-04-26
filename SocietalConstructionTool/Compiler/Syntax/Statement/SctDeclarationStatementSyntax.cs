namespace Sct.Compiler.Syntax
{
    public class SctDeclarationStatementSyntax(
        SctTypeSyntax type,
        string id,
        SctExpressionSyntax expression
    ) : SctStatementSyntax
    {
        public SctTypeSyntax Type => type;
        public string Id => id;
        public SctExpressionSyntax Expression => expression;
    }
}

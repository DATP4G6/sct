namespace Sct.Compiler.Syntax
{
    public class SctNamedArgumentSyntax(SctSyntaxContext context, string id, SctExpressionSyntax expression) : SctDefinitionSyntax(context)
    {
        public string Id => id;
        public SctExpressionSyntax Expression => expression;
        public override IEnumerable<SctSyntax> Children => [Expression];
    }
}

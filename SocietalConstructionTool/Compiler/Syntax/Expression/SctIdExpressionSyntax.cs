namespace Sct.Compiler.Syntax
{
    public class SctIdExpressionSyntax(SctSyntaxContext context, string id) : SctExpressionSyntax(context)
    {
        public string Id => id;
        public override IEnumerable<SctSyntax> Children => [];
    }
}

namespace Sct.Compiler.Syntax
{
    public class SctCallExpressionSyntax(SctSyntaxContext context, string target, IEnumerable<SctExpressionSyntax> expressions) : SctExpressionSyntax(context)
    {
        public string Target => target;
        public IEnumerable<SctExpressionSyntax> Expressions => expressions;
        public override IEnumerable<SctSyntax> Children => [.. Expressions];
    }
}

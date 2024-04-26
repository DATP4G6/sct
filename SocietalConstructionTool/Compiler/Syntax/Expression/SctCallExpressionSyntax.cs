namespace Sct.Compiler.Syntax
{
    public class SctCallExpressionSyntax(string target, IEnumerable<SctExpressionSyntax> expressions) : SctExpressionSyntax
    {
        public string Target => target;
        public IEnumerable<SctExpressionSyntax> Expressions => expressions;
    }
}

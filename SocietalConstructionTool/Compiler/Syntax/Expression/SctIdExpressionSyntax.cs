namespace Sct.Compiler.Syntax
{
    public class SctIdExpressionSyntax(string id) : SctExpressionSyntax
    {
        public string Id => id;
    }
}

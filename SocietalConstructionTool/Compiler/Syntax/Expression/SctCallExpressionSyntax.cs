using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctCallExpressionSyntax(ParserRuleContext context, string target, IEnumerable<SctExpressionSyntax> expressions) : SctExpressionSyntax(context)
    {
        public string Target => target;
        public IEnumerable<SctExpressionSyntax> Expressions => expressions;
        public override IEnumerable<SctSyntax> Children => [.. Expressions];
    }
}

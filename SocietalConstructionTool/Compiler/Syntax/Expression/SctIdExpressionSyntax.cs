using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctIdExpressionSyntax(ParserRuleContext context, string id) : SctExpressionSyntax(context)
    {
        public string Id => id;
        public override IEnumerable<SctSyntax> Children => [];
    }
}

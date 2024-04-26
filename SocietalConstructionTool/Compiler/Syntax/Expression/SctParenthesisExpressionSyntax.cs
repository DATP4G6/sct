using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctParenthesisExpressionSyntax(ParserRuleContext context, SctExpressionSyntax expression) : SctExpressionSyntax(context)
    {
        public SctExpressionSyntax Expression => expression;
        public override IEnumerable<SctSyntax> Children => [Expression];
    }
}

using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctTypecastExpressionSyntax(ParserRuleContext context, SctTypeSyntax type, SctExpressionSyntax expression) : SctExpressionSyntax(context)
    {
        public SctTypeSyntax Type => type;
        public SctExpressionSyntax Expression => expression;
        public override IEnumerable<SctSyntax> Children => [Type, Expression];
    }
}

using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctReturnStatementSyntax(ParserRuleContext context, SctExpressionSyntax? expression) : SctStatementSyntax(context)
    {
        public SctExpressionSyntax? Expression => expression;
        public override IEnumerable<SctSyntax> Children => Expression is null ? [] : [Expression];
    }
}

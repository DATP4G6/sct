using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctWhileStatementSyntax(ParserRuleContext context, SctExpressionSyntax expression, SctBlockStatementSyntax block) : SctStatementSyntax(context)
    {
        public SctExpressionSyntax Expression => expression;
        public SctBlockStatementSyntax Block => block;
        public override IEnumerable<SctSyntax> Children => [Expression, Block];
    }
}

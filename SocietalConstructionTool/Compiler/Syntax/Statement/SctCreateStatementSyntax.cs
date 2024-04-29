using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctCreateStatementSyntax(ParserRuleContext context, SctAgentExpressionSyntax agent) : SctStatementSyntax(context)
    {
        public SctAgentExpressionSyntax Agent => agent;
        public override IEnumerable<SctSyntax> Children => [Agent];
    }
}

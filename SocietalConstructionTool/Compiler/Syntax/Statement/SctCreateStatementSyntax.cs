namespace Sct.Compiler.Syntax
{
    public class SctCreateStatementSyntax(SctAgentExpressionSyntax agent) : SctStatementSyntax
    {
        public SctAgentExpressionSyntax Agent => agent;
    }
}

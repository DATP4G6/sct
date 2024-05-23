namespace Sct.Compiler.Syntax
{
    public class SctCreateStatementSyntax(SctSyntaxContext context, SctAgentExpressionSyntax agent) : SctStatementSyntax(context)
    {
        public SctAgentExpressionSyntax Agent => agent;
        public override IEnumerable<SctSyntax> Children => [Agent];
    }
}

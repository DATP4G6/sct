namespace Sct.Compiler.Syntax
{
    public class SctAssignmentStatementSyntax(string id, SctExpressionSyntax expression) : SctStatementSyntax
    {
        public string Id => id;
        public SctExpressionSyntax Expression => expression;
    }
}

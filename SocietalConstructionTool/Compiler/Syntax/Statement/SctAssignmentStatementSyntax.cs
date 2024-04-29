using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctAssignmentStatementSyntax(ParserRuleContext context, string id, SctExpressionSyntax expression) : SctStatementSyntax(context)
    {
        public string Id => id;
        public SctExpressionSyntax Expression => expression;
        public override IEnumerable<SctSyntax> Children => [Expression];
    }
}

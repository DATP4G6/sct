namespace Sct.Compiler.Syntax
{
    public class SctIfStatementSyntax(SctSyntaxContext context, SctExpressionSyntax expression, SctBlockStatementSyntax block, SctElseStatementSyntax? @else) : SctStatementSyntax(context)
    {
        public SctExpressionSyntax Expression => expression;
        public SctBlockStatementSyntax Then => block;
        public SctElseStatementSyntax? Else => @else;
        public override IEnumerable<SctSyntax> Children => Else is null ? [Expression, Then] : [Expression, Then, Else];
    }
}

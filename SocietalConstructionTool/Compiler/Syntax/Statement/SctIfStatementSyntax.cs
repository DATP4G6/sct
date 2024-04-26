namespace Sct.Compiler.Syntax
{
    public class SctIfStatementSyntax(SctExpressionSyntax expression, SctBlockStatementSyntax block, SctElseStatementSyntax? @else) : SctStatementSyntax
    {
        public SctExpressionSyntax Expression => expression;
        public SctBlockStatementSyntax Block => block;
        public SctElseStatementSyntax? Else => @else;
    }
}

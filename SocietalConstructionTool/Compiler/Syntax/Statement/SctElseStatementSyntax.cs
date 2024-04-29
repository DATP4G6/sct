using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctElseStatementSyntax(ParserRuleContext context, SctBlockStatementSyntax block) : SctStatementSyntax(context)
    {
        public SctBlockStatementSyntax Block => block;
        public override IEnumerable<SctSyntax> Children => [Block];
    }
}

using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctBreakStatementSyntax(ParserRuleContext context) : SctStatementSyntax(context)
    {
        public override IEnumerable<SctSyntax> Children => [];
    }
}

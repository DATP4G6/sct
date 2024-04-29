using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctExitStatementSyntax(ParserRuleContext context) : SctStatementSyntax(context)
    {
        public override IEnumerable<SctSyntax> Children => [];
    }
}

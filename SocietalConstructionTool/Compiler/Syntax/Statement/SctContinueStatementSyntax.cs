using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctContinueStatementSyntax(ParserRuleContext context) : SctStatementSyntax(context)
    {
        public override IEnumerable<SctSyntax> Children => [];
    }
}

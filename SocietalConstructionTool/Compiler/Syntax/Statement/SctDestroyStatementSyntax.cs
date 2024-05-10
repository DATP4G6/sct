using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctDestroyStatementSyntax(ParserRuleContext context) : SctStatementSyntax(context)
    {
        public override IEnumerable<SctSyntax> Children => [];
    }
}

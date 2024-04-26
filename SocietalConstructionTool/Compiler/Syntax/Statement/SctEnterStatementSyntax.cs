using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctEnterStatementSyntax(ParserRuleContext context, string id) : SctStatementSyntax(context)
    {
        public string Id => id;
        public override IEnumerable<SctSyntax> Children => [];
    }
}

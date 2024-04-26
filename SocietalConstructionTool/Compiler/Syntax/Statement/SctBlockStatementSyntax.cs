using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctBlockStatementSyntax(ParserRuleContext context, IEnumerable<SctStatementSyntax> statements) : SctStatementSyntax(context)
    {
        public IEnumerable<SctStatementSyntax> Statements => statements;
        public override IEnumerable<SctSyntax> Children => [.. Statements];
    }
}

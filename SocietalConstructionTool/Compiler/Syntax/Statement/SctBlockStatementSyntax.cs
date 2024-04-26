namespace Sct.Compiler.Syntax
{
    public class SctBlockStatementSyntax(IEnumerable<SctStatementSyntax> statements) : SctStatementSyntax
    {
        public IEnumerable<SctStatementSyntax> Statements => statements;
    }
}

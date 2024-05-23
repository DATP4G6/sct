namespace Sct.Compiler.Syntax
{
    public class SctBlockStatementSyntax(SctSyntaxContext context, IEnumerable<SctStatementSyntax> statements) : SctStatementSyntax(context)
    {
        public IEnumerable<SctStatementSyntax> Statements => statements;
        public override IEnumerable<SctSyntax> Children => [.. Statements];
    }
}

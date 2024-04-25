namespace Sct.Compiler.Syntax
{
    public class SctEnterStatementSyntax(string id) : SctStatementSyntax
    {
        public string Id => id;
    }
}

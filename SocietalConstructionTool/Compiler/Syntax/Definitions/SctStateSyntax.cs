namespace Sct.Compiler.Syntax
{
    public class SctStateSyntax(string id, IEnumerable<string> decorations, SctBlockStatementSyntax block) : SctDefinitionSyntax
    {
        public string Id => id;
        public IEnumerable<string> Decorations => decorations;
        public SctBlockStatementSyntax Block => block;
    }
}

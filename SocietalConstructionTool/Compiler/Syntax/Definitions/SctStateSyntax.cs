namespace Sct.Compiler.Syntax
{
    public class SctStateSyntax(SctSyntaxContext context, string id, IEnumerable<string> decorations, SctBlockStatementSyntax block) : SctDefinitionSyntax(context)
    {
        public string Id => id;
        public IEnumerable<string> Decorations => decorations;
        public SctBlockStatementSyntax Block => block;
        public override IEnumerable<SctSyntax> Children => [Block];
    }
}

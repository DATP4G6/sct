namespace Sct.Compiler.Syntax
{
    public class SctParameterSyntax(SctSyntaxContext context, SctTypeSyntax type, string id) : SctDefinitionSyntax(context)
    {
        public SctTypeSyntax Type => type;
        public string Id => id;
        public override IEnumerable<SctSyntax> Children => [Type];
    }
}

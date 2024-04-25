namespace Sct.Compiler.Syntax
{
    public class SctParameterSyntax(SctTypeSyntax type, string id) : SctDefinitionSyntax
    {
        public SctTypeSyntax Type => type;
        public string Id => id;
    }
}

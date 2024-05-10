using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctParameterSyntax(ParserRuleContext context, SctTypeSyntax type, string id) : SctDefinitionSyntax(context)
    {
        public SctTypeSyntax Type => type;
        public string Id => id;
        public override IEnumerable<SctSyntax> Children => [Type];
    }
}

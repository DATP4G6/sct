using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public enum SctType
    {
        Int,
        Float,
        Predicate,
        Void
    }

    public class SctTypeSyntax(ParserRuleContext context, SctType type) : SctDefinitionSyntax(context)
    {
        public SctType Type => type;
        public override IEnumerable<SctSyntax> Children => [];
    }
}

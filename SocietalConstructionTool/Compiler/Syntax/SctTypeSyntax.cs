namespace Sct.Compiler.Syntax
{
    public enum SctType
    {
        Int,
        Float,
        Predicate,
        Void
    }

    public class SctTypeSyntax(SctType type) : SctDefinitionSyntax
    {
        public SctType Type => type;
    }
}

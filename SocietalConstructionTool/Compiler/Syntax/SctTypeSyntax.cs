namespace Sct.Compiler.Syntax
{
    public enum SctType
    {
        Int,
        Float,
        Predicate,
        Void,
        Ok
    }

    public static class SctTypeMethods
    {
        public static string TypeName(this SctType t) => t switch
        {
            SctType.Int or SctType.Float or SctType.Void => t.ToString().ToLowerInvariant(),
            SctType.Predicate or SctType.Ok => t.ToString(),
            _ => t.ToString(),
        };
    }

    public class SctTypeSyntax(SctSyntaxContext context, SctType type) : SctDefinitionSyntax(context)
    {
        public SctType Type => type;
        public override IEnumerable<SctSyntax> Children => [];
    }
}

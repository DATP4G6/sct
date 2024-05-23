namespace Sct.Compiler.Syntax
{
    public class SctAgentExpressionSyntax(
        SctSyntaxContext context,
        string className,
        string stateName,
        IEnumerable<SctNamedArgumentSyntax> fields
    ) : SctExpressionSyntax(context)
    {
        public string ClassName => className;
        public string StateName => stateName;
        public IEnumerable<SctNamedArgumentSyntax> Fields => fields;
        public override IEnumerable<SctSyntax> Children => [.. Fields];
    }
}

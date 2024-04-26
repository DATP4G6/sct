namespace Sct.Compiler.Syntax
{
    public class SctAgentExpressionSyntax(
        string className,
        string stateName,
        IDictionary<string, SctExpressionSyntax> fields
    ) : SctExpressionSyntax
    {
        public string ClassName => className;
        public string StateName => stateName;
        public IDictionary<string, SctExpressionSyntax> Fields => fields;
    }
}

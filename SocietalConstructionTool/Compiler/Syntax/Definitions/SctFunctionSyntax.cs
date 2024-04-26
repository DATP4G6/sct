namespace Sct.Compiler.Syntax
{
    public class SctFunctionSyntax(
        string id,
        IEnumerable<SctParameterSyntax> parameters,
        SctTypeSyntax returnType,
        SctBlockStatementSyntax block
    ) : SctDefinitionSyntax
    {
        public string Id => id;
        public IEnumerable<SctParameterSyntax> Parameters => parameters;
        public SctTypeSyntax ReturnType => returnType;
        public SctBlockStatementSyntax Block => block;
    }
}

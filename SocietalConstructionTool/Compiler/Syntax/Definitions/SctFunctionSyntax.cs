namespace Sct.Compiler.Syntax
{
    public class SctFunctionSyntax(
        SctSyntaxContext context,
        string id,
        IEnumerable<SctParameterSyntax> parameters,
        SctTypeSyntax returnType,
        SctBlockStatementSyntax block
    ) : SctDefinitionSyntax(context)
    {
        public string Id => id;
        public IEnumerable<SctParameterSyntax> Parameters => parameters;
        public SctTypeSyntax ReturnType => returnType;
        public SctBlockStatementSyntax Block => block;
        public override IEnumerable<SctSyntax> Children => [.. Parameters, ReturnType, Block];
    }
}

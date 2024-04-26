namespace Sct.Compiler.Syntax
{
    public class SctClassSyntax(
            string id,
            IEnumerable<SctParameterSyntax> parameters,
            IEnumerable<SctDecoratorSyntax> decorators,
            IEnumerable<SctFunctionSyntax> functions,
            IEnumerable<SctStateSyntax> states
    ) : SctDefinitionSyntax
    {
        public string Id => id;
        public IEnumerable<SctParameterSyntax> Parameters => parameters;
        public IEnumerable<SctDecoratorSyntax> Decorators => decorators;
        public IEnumerable<SctStateSyntax> States => states;
        public IEnumerable<SctFunctionSyntax> Functions => functions;
    }
}

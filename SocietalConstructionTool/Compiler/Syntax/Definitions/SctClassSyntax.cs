using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctClassSyntax(
            ParserRuleContext context,
            string id,
            IEnumerable<SctParameterSyntax> parameters,
            IEnumerable<SctDecoratorSyntax> decorators,
            IEnumerable<SctFunctionSyntax> functions,
            IEnumerable<SctStateSyntax> states
    ) : SctDefinitionSyntax(context)
    {
        public string Id => id;
        public IEnumerable<SctParameterSyntax> Parameters => parameters;
        public IEnumerable<SctDecoratorSyntax> Decorators => decorators;
        public IEnumerable<SctStateSyntax> States => states;
        public IEnumerable<SctFunctionSyntax> Functions => functions;
        public override IEnumerable<SctSyntax> Children => [.. Parameters, .. Decorators, .. Functions, .. States];
    }
}

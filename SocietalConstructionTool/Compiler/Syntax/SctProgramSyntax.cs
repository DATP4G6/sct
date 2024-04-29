using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctProgramSyntax(ParserRuleContext context, IEnumerable<SctFunctionSyntax> functions, IEnumerable<SctClassSyntax> classes) : SctSyntax(context)
    {
        public IEnumerable<SctFunctionSyntax> Functions { get; } = functions;
        public IEnumerable<SctClassSyntax> Classes { get; } = classes;
        public override IEnumerable<SctSyntax> Children => [.. Functions, .. Classes];
    }
}

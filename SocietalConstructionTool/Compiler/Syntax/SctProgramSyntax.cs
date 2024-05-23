namespace Sct.Compiler.Syntax
{
    public class SctProgramSyntax(SctSyntaxContext context, IEnumerable<SctFunctionSyntax> functions, IEnumerable<SctClassSyntax> classes) : SctSyntax(context)
    {
        public IEnumerable<SctFunctionSyntax> Functions { get; } = functions;
        public IEnumerable<SctClassSyntax> Classes { get; } = classes;
        public override IEnumerable<SctSyntax> Children => [.. Functions, .. Classes];
    }
}

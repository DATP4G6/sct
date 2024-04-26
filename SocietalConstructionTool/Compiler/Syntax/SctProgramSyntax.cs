namespace Sct.Compiler.Syntax
{
    public class SctProgramSyntax(IEnumerable<SctFunctionSyntax> functions, IEnumerable<SctClassSyntax> classes) : SctSyntax
    {
        public IEnumerable<SctFunctionSyntax> Functions { get; } = functions;
        public IEnumerable<SctClassSyntax> Classes { get; } = classes;
    }
}

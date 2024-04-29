namespace Sct.Compiler.Typechecker
{
    public class FunctionType(Syntax.SctType returnType, List<Syntax.SctType> parameterTypes)
    {
        public Syntax.SctType ReturnType { get; } = returnType;
        public List<Syntax.SctType> ParameterTypes { get; } = parameterTypes;
    }
}

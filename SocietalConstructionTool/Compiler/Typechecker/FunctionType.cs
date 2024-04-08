namespace Sct.Compiler.Typechecker
{
    public class FunctionType(SctType returnType, List<SctType> parameterTypes)
    {
        public SctType ReturnType { get; } = returnType;
        public List<SctType> ParameterTypes { get; } = parameterTypes;
    }
}

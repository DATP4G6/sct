namespace Sct.Compiler
{
    public class FunctionType
    {
        public SctType ReturnType { get; set; }
        public List<SctType> ParameterTypes { get; set; }

        public FunctionType(SctType returnType, List<SctType> parameterTypes)
        {
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
        }
    }


}


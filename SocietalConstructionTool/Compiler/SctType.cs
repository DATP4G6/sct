namespace Sct.Compiler
{
    public class SctType(Type targetType, string name)
    {
        public Type TargetType { get; } = targetType;
        public string TypeName { get; } = name;
    }
}

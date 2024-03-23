namespace Sct.Compiler
{
    public class SctType(Type targetType)
    {
        public Type TargetType { get; } = targetType;
        public override string ToString()
        {
            return TargetType.Name;
        }
    }
}

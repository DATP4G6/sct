namespace Sct.Compiler
{
    public class SctType(Type targetType, string name)
    {
        public Type TargetType { get; } = targetType;
        public string TypeName { get; } = name;

        // Temporary casts to make type checker work
        public static implicit operator SctType(Syntax.SctType t)
        {
            return t switch
            {
                Syntax.SctType.Int => TypeTable.Int,
                Syntax.SctType.Float => TypeTable.Float,
                Syntax.SctType.Predicate => TypeTable.Predicate,
                Syntax.SctType.Void => TypeTable.Void,
                Syntax.SctType.Ok => TypeTable.Ok,
                _ => throw new InvalidCastException($"Could not cast {t} to TypeTable type"),
            };
        }

        public static implicit operator Syntax.SctType(SctType t)
        {
            return t switch
            {
                { } when t == TypeTable.Int => Syntax.SctType.Int,
                { } when t == TypeTable.Float => Syntax.SctType.Float,
                { } when t == TypeTable.Predicate => Syntax.SctType.Predicate,
                { } when t == TypeTable.Void => Syntax.SctType.Void,
                { } when t == TypeTable.Ok => Syntax.SctType.Ok,
                _ => throw new InvalidCastException($"Could not cast {t} to syntax type"),
            };
        }
    }
}

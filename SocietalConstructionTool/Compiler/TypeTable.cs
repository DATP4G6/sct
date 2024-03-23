using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Compiler.Exceptions;

namespace Sct.Compiler
{
    public static class TypeTable
    {
        private static readonly Dictionary<string, SctType> Types = new()
        {
            { "int", new SctType(typeof(int)) },
            { "float", new SctType(typeof(double)) },
            { "void", new SctType(typeof(void)) },
            { "Predicate", new SctType(typeof(void)) },
        };
        private static readonly Dictionary<SctType, SctType[]> CompatibleTypes = new()
        {
            { Int, [Int, Float] },
            { Float, [Float] },
            { Predicate, [Predicate] },
        };
        private static readonly Dictionary<SctType, SctType[]> ExplicitlyCompatibleTypes = new()
        {
            { Int, [Int, Float] },
            { Float, [Int, Float] },
            { Predicate, [Predicate] },
        };
        // private static Dictionary<SctType, SctType> ExplicitlyCompatibleTypes => CompatibleTypes.Concat(new Dictionary<SctType, SctType>()
        // {
        //     { Int, Int },
        //     { Float, Float },
        //     { Predicate, Predicate },
        //     { Int, Float },
        // }).ToDictionary();
        // public static SctType GetType(string name) => Types[name] ?? throw new InvalidTypeException($"Type {name} does not exist");
        public static SctType GetType(string name)
        {
            if (Types.TryGetValue(name, out SctType? type) && type != null)
            {
                return type;
            }
            throw new InvalidTypeException($"Type {name} does not exist");
        }
        public static SctType Void => Types["void"];
        public static SctType Int => Types["int"];
        public static SctType Float => Types["float"];
        public static SctType Predicate => Types["Predicate"];
        public static bool IsNumber(string type) => IsNumber(GetType(type));
        public static bool IsNumber(SctType type) => type == Int || type == Float;
        public static bool IsCompatible(string from, string to) => IsCompatible(GetType(from), GetType(to));
        public static bool IsCompatible(string from, SctType to) => IsCompatible(GetType(from), to);
        public static bool IsCompatible(SctType from, string to) => IsCompatible(from, GetType(to));
        public static bool IsCompatible(SctType from, SctType to)
        {
            if (CompatibleTypes.TryGetValue(from, out SctType[]? compatibleTypes))
            {
                return compatibleTypes?.Contains(to) ?? false;
            }
            return false;
        }
        public static bool IsExplicitlyCompatible(string from, string to) => IsExplicitlyCompatible(GetType(from), GetType(to));
        public static bool IsExplicitlyCompatible(string from, SctType to) => IsExplicitlyCompatible(GetType(from), to);
        public static bool IsExplicitlyCompatible(SctType from, string to) => IsExplicitlyCompatible(from, GetType(to));
        public static bool IsExplicitlyCompatible(SctType from, SctType to)
        {
            if (ExplicitlyCompatibleTypes.TryGetValue(from, out SctType[]? compatibleTypes))
            {
                return compatibleTypes?.Contains(to) ?? false;
            }
            return false;
        }

        public static TypeSyntax GetTypeNode(string name)
        {
            SctType type = (Types[name]) ?? throw new InvalidTypeException($"Type {name} does not exist");
            if (type == Types["Predicate"])
            {
                throw new InvalidTypeException("Predicate type cannot be used as a syntax node");
            }
            string typeName = type.TargetType.Name;
            return SyntaxFactory.ParseTypeName(typeName);
        }

    }
}

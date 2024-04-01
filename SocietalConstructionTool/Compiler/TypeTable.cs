using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Compiler.Exceptions;

namespace Sct.Compiler
{
    public static class TypeTable
    {
        public static SctType Void => _types["void"];
        public static SctType Int => _types["int"];
        public static SctType Float => _types["float"];
        public static SctType Predicate => _types["Predicate"];
        private static readonly Dictionary<string, SctType> _types = new()
        {
            { "int", new SctType(typeof(int)) },
            { "float", new SctType(typeof(double)) },
            { "void", new SctType(typeof(void)) },
            { "Predicate", new SctType(typeof(void)) },
        };

        public static SctType? GetType(string name) => _types[name];

        public static TypeSyntax GetTypeNode(string name)
        {
            SctType type = (_types[name]) ?? throw new InvalidTypeException($"Type {name} does not exist");
            if (type == _types["Predicate"])
            {
                throw new InvalidTypeException("Predicate type cannot be used as a syntax node");
            }
            string typeName = type.TargetType.Name;
            return SyntaxFactory.ParseTypeName(typeName);
        }

        public static bool TypeIsNumeric(SctType type) => type == Int || type == Float;

        public static bool IsTypeCastable(SctType from, SctType to) => from == to || (TypeIsNumeric(from) && TypeIsNumeric(to));
    }
}

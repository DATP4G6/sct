using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Compiler.Exceptions;

namespace Sct.Compiler
{
    public static class TypeTable
    {
        public static SctType Void => Types["void"];
        public static SctType Int => Types["int"];
        public static SctType Float => Types["float"];
        public static SctType Predicate => Types["Predicate"];
        private static readonly Dictionary<string, SctType> Types = new()
        {
            { "int", new SctType(typeof(int)) },
            { "float", new SctType(typeof(double)) },
            { "void", new SctType(typeof(void)) },
            { "Predicate", new SctType(typeof(void)) },
        };

        public static SctType? GetType(string name) => Types[name];

        public static TypeSyntax GetTypeNode(string name)
        {
            SctType sctType = (Types[name]) ?? throw new InvalidTypeException($"Type {name} does not exist");
            if (sctType == Types["Predicate"])
            {
                throw new InvalidTypeException("Predicate type cannot be used as a syntax node");
            }

            var syntaxKind = name switch
            {
                "int" => SyntaxKind.LongKeyword,
                "float" => SyntaxKind.DoubleKeyword,
                "void" => SyntaxKind.VoidKeyword,
                _ => throw new InvalidTypeException($"Got unknown type in translation: ${name}")
            };

            var @type = SyntaxFactory.PredefinedType(SyntaxFactory.Token(syntaxKind));

            return @type;
        }

        public static bool TypeIsNumeric(SctType type) => type == Int || type == Float;

        public static bool IsTypeCastable(SctType from, SctType to) => from == to || (TypeIsNumeric(from) && TypeIsNumeric(to));
    }
}

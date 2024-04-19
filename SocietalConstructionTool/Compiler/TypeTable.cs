using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Compiler.Exceptions;
using Sct.Runtime;

namespace Sct.Compiler
{
    public static class TypeTable
    {
        public static SctType Void => Types["void"];
        public static SctType Int => Types["int"];
        public static SctType Float => Types["float"];
        public static SctType Predicate => Types["Predicate"];
        public static SctType Ok => Types["ok"];
        private static readonly Dictionary<string, SctType> Types = new()
        {
            { "int", new SctType(typeof(int), "int") },
            { "float", new SctType(typeof(double), "float") },
            { "void", new SctType(typeof(void), "void")},
            { "Predicate", new SctType(typeof(QueryPredicate), "Predicate") },
            { "ok", new SctType(typeof(void), "ok")}
        };

        public static SctType? GetType(string name) => Types[name];

        public static TypeSyntax GetTypeNode(string name)
        {
            if (Types[name] is null) throw new InvalidTypeException($"Type {name} does not exist");
            static PredefinedTypeSyntax PredefinedType(SyntaxKind kind) => SyntaxFactory.PredefinedType(SyntaxFactory.Token(kind));
            var @type = name switch
            {
                "int" => PredefinedType(SyntaxKind.LongKeyword),
                "float" => PredefinedType(SyntaxKind.DoubleKeyword),
                "void" => PredefinedType(SyntaxKind.VoidKeyword),
                _ => SyntaxFactory.ParseTypeName(Types[name].TargetType.Name)
            };

            return @type;
        }

        public static SctType? GetCompatibleType(SctType left, SctType right)
        {
            if (left == right)
            {
                return left;
            }
            else if (left == Float && right == Int)
            {
                return Float;
            }
            return null;
        }

        public static bool TypeIsNumeric(SctType type) => type == Int || type == Float;

        public static bool IsTypeCastable(SctType from, SctType to) => from == to || (TypeIsNumeric(from) && TypeIsNumeric(to));
    }
}

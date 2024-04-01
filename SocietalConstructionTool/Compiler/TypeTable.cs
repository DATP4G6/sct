using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Compiler.Exceptions;

namespace Sct.Compiler
{
    public class TypeTable
    {
        public SctType Void => _types["void"];
        public SctType Int => _types["int"];
        public SctType Float => _types["float"];
        public SctType Predicate => _types["Predicate"];
        private readonly Dictionary<string, SctType> _types = new()
        {
            { "int", new SctType(typeof(int)) },
            { "float", new SctType(typeof(double)) },
            { "void", new SctType(typeof(void)) },
            { "Predicate", new SctType(typeof(void)) },
        };

        public SctType? GetType(string name) => _types[name];

        public TypeSyntax GetTypeNode(string name)
        {
            SctType sctType = (_types[name]) ?? throw new InvalidTypeException($"Type {name} does not exist");
            if (sctType == _types["Predicate"])
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

        public bool TypeIsNumeric(SctType type) => type == Int || type == Float;

        public bool IsTypeCastable(SctType from, SctType to) => from == to || (TypeIsNumeric(from) && TypeIsNumeric(to));
    }
}

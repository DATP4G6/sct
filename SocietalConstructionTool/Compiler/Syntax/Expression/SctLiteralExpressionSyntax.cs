using System.Numerics;

namespace Sct.Compiler.Syntax
{
    public class SctLiteralExpressionSyntax<T>(SctTypeSyntax type, T value) : SctExpressionSyntax where T : INumber<T>
    {
        public SctTypeSyntax Type => type;
        public T Value => value;
    }
}

using System.Numerics;

namespace Sct.Compiler.Syntax
{
    public abstract class AbstractSctLiteralExpressionSyntax(SctSyntaxContext context, SctType type) : SctExpressionSyntax(context)
    {
        public SctType Type { get; } = type;
    }

    public class SctLiteralExpressionSyntax<T>(SctSyntaxContext context, SctType type, T value) : AbstractSctLiteralExpressionSyntax(context, type) where T : INumber<T>
    {
        public T Value { get; } = value;
        public override IEnumerable<SctSyntax> Children => [];
    }
}

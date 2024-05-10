using System.Numerics;

using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public abstract class AbstractSctLiteralExpressionSyntax : SctExpressionSyntax
    {
        public SctType Type { get; }

        public AbstractSctLiteralExpressionSyntax(ParserRuleContext context, SctType type)
            : base(context)
        {
            Type = type;
        }

        public AbstractSctLiteralExpressionSyntax(SctSyntaxContext context, SctType type)
            : base(context)
        {
            Type = type;
        }
    }

    public class SctLiteralExpressionSyntax<T> : AbstractSctLiteralExpressionSyntax where T : INumber<T>
    {
        public T Value { get; }
        public override IEnumerable<SctSyntax> Children => [];

        public SctLiteralExpressionSyntax(ParserRuleContext context, SctType type, T value)
            : base(context, type)
        {
            Value = value;
        }

        public SctLiteralExpressionSyntax(SctSyntaxContext context, SctType type, T value)
            : base(context, type)
        {
            Value = value;
        }
    }
}

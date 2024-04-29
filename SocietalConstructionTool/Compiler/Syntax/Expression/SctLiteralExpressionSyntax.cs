using Antlr4.Runtime;

using System.Numerics;

namespace Sct.Compiler.Syntax
{
    public class SctLiteralExpressionSyntax<T>(ParserRuleContext context, SctType type, T value) : SctExpressionSyntax(context) where T : INumber<T>
    {
        public SctType Type => type;
        public T Value => value;
        public override IEnumerable<SctSyntax> Children => [];
    }
}

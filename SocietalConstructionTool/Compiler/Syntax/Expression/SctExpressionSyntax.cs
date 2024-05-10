using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public abstract class SctExpressionSyntax : SctSyntax
    {
        public SctExpressionSyntax(ParserRuleContext context) : base(context) { }
        public SctExpressionSyntax(SctSyntaxContext context) : base(context) { }
    }
}

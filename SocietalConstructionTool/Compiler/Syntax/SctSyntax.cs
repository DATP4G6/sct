using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctSyntaxContext
    {
        public int Line { get; }
        public int Column { get; }

        public SctSyntaxContext(ParserRuleContext context)
        {
            Line = context.Start.Line;
            Column = context.Start.Column;
        }
    }
    public abstract class SctSyntax
    {
        public T Accept<T>(SctBaseSyntaxVisitor<T> visitor) => visitor.Visit(this);
        public abstract IEnumerable<SctSyntax> Children { get; }
        public SctSyntaxContext Context { get; }

        protected SctSyntax(ParserRuleContext context)
        {
            Context = new SctSyntaxContext(context);
        }
    }
}

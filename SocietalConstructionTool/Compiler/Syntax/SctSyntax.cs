using System.Text.Json.Serialization;

using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctSyntaxContext
    {
        public int Line { get; }
        public int Column { get; }

        public ParserRuleContext OriginalContext { get; }

        public SctSyntaxContext(ParserRuleContext context)
        {
            OriginalContext = context;
            Line = context.Start.Line;
            Column = context.Start.Column;
        }
    }
    public abstract class SctSyntax
    {
        public T Accept<T>(SctBaseSyntaxVisitor<T> visitor) => visitor.Visit(this);

        [JsonIgnore]
        public abstract IEnumerable<SctSyntax> Children { get; }

        [JsonIgnore]
        public SctSyntaxContext Context { get; set; }

        protected SctSyntax(ParserRuleContext context)
        {
            Context = new SctSyntaxContext(context);
        }

        protected SctSyntax(SctSyntaxContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Forces evaluation of the abstract syntax tree
        ///
        /// Without this, the AST is lazily evaluated,
        /// which can lead to errors not being caught in test,
        /// as the nodes may never be visited.
        /// </summary>
        public void ForceEvaluation()
        {
            foreach (var child in Children.ToList())
            {
                child.ForceEvaluation();
            }
        }
    }
}

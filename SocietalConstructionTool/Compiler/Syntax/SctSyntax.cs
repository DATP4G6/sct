using System.Text.Json.Serialization;

using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctSyntaxContext(ParserRuleContext context)
    {
        public int Line { get; } = context.Start.Line;
        public int Column { get; } = context.Start.Column;
        public string? Filename { get; set; }

        public ParserRuleContext OriginalContext { get; } = context;
    }

    public abstract class SctSyntax
    {
        public T Accept<T>(SctBaseSyntaxVisitor<T> visitor) => visitor.Visit(this);

        [JsonIgnore]
        public abstract IEnumerable<SctSyntax> Children { get; }

        [JsonIgnore]
        public SctSyntaxContext Context { get; }

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

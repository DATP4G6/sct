using System.Text.Json.Serialization;

using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctSyntaxContext(ParserRuleContext context)
    {
        public int Line { get; } = context.Start.Line;
        public int Column { get; } = context.Start.Column;
        public string? Filename { get; set; }

        // add implicit typecast from antlr context to our own context
        public static implicit operator SctSyntaxContext(ParserRuleContext context)
        {
            return new SctSyntaxContext(context);
        }
    }

    public abstract class SctSyntax(SctSyntaxContext context)
    {
        public T Accept<T>(SctBaseSyntaxVisitor<T> visitor) => visitor.Visit(this);

        [JsonIgnore]
        public abstract IEnumerable<SctSyntax> Children { get; }

        [JsonIgnore]
        public SctSyntaxContext Context { get; } = context;

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

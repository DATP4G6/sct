namespace Sct.Compiler.Syntax
{
    public abstract class SctSyntax
    {
        public T Accept<T>(SctBaseSyntaxVisitor<T> visitor) => visitor.Visit(this);
    }
}

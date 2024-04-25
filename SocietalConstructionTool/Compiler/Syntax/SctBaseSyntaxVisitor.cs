namespace Sct.Compiler.Syntax
{
    public class SctBaseSyntaxVisitor<T>
    {
        public T Visit(SctSyntax node) => throw new InvalidOperationException("SctSyntax node was not recognized");
        // TODO: Add default Visit implementations for all SctSyntax derivatives
    }
}

namespace Sct.Compiler
{
    public interface IErrorReporter
    {
        IEnumerable<CompilerError> Errors { get; }
    }
}


using Antlr4.Runtime;

namespace Sct.Compiler
{
    public class SctErrorListener : BaseErrorListener, IErrorReporter
    {
        private readonly List<CompilerError> _errors = new();
        public IEnumerable<CompilerError> Errors => _errors;

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            _errors.Add(new CompilerError(msg, line, charPositionInLine));
        }

    }
}


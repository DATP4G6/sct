namespace Sct.Compiler.Exceptions
{
    public class UnrecognizedNodeException : Exception
    {
        public UnrecognizedNodeException()
        {
        }

        public UnrecognizedNodeException(string message) : base(message)
        {
        }

        public UnrecognizedNodeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UnrecognizedNodeException(string expected, string actual) : base($"Expected {expected} but got {actual}")
        {
        }
    }
}

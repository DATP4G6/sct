namespace Sct.Compiler.Exceptions
{
    public class UnrecognizedValueException : Exception
    {
        public UnrecognizedValueException()
        {
        }

        public UnrecognizedValueException(string message) : base(message)
        {
        }

        public UnrecognizedValueException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UnrecognizedValueException(string expected, string actual) : base($"Expected {expected} but got {actual}")
        {
        }
    }
}

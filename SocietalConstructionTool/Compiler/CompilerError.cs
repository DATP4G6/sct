namespace Sct.Compiler
{
    public class CompilerError
    {
        public string Message { get; }
        public int? Line { get; }
        public int? Column { get; }

        public CompilerError(string message)
        {
            Message = message;
        }

        public CompilerError(string message, int line)
        {
            Message = message;
            Line = line;
        }

        public CompilerError(string message, int line, int column)
        {
            Message = message;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            if (Line is null)
            {
                return Message;
            }
            if (Column is null)
            {
                return $"Line {Line}: {Message}";
            }
            return $"Line {Line}, Column {Column}: {Message}";
        }
    }
}

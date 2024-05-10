using Sct.Compiler.Syntax;

namespace Sct.Compiler
{
    public class CompilerError
    {
        public string Message { get; }
        public int? Line { get; }
        public int? Column { get; }
        public string? Filename { get; set; }

        public CompilerError(string message)
        {
            Message = message;
        }

        public CompilerError(string message, int line, int column)
        {
            Message = message;
            Line = line;
            Column = column;
        }

        public CompilerError(string message, SctSyntaxContext context)
        {
            Message = message;
            Filename = context.Filename;
            Line = context.Line;
            Column = context.Column;
        }

        public override string ToString()
        {
            if (Filename is not null)
            {
                if (Line is null)
                {
                    return $"{Filename}: {Message}";
                }
                if (Column is null)
                {
                    return $"{Filename}, Line {Line}: {Message}";
                }
                return $"{Filename}, Line {Line}, Column {Column}: {Message}";
            }

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

namespace Sct.Compiler.Typechecker
{
    public class VtableEntry
    {
        public string Name { get; }
        public SctType Type { get; }

        public VtableEntry(string name, SctType type)
        {
            Name = name;
            Type = type;
        }
    }
}

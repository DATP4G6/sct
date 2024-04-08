namespace Sct.Compiler.Typechecker
{
    public class VTableEntry(string name, SctType type)
    {
        public string Name { get; } = name;
        public SctType Type { get; } = type;
    }
}

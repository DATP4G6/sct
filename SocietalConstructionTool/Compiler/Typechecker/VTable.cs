namespace Sct.Compiler.Typechecker
{
    public class VTable
    {
        private readonly StackAdapter<VTableEntry> _entries = new();

        public bool AddEntry(string name, SctType type)
        {
            if (_entries.Any(x => x.Name == name))
            {
                return false;
            }
            _entries.Push(new VTableEntry(name, type));
            return true;
        }

        public Syntax.SctType? Lookup(string name) => _entries.FirstOrDefault(x => x.Name == name)?.Type;

        public void EnterScope() => _entries.PushMarker();

        public void ExitScope() => _ = _entries.PopUntilMarker();

        private sealed class VTableEntry(string name, Syntax.SctType type)
        {
            public string Name { get; } = name;
            public Syntax.SctType Type { get; } = type;
        }
    }
}

using Sct.Compiler.Exceptions;

namespace Sct.Compiler
{
    public class Vtable
    {
        private readonly StackAdapter<VtableEntry> _entries = new StackAdapter<VtableEntry>();

        public void AddEntry(string name, SctType type)
        {
            _entries.Push(new VtableEntry(name, type));
        }

        public SctType Lookup(string name)
        {
            return _entries.Where(x => x.Name == name)
                    .FirstOrDefault()?
                    .Type ?? throw new UnrecognizedNodeException("Vtable entry not found: " + name);
        }

        public void EnterScope()
        {
            _entries.PushMarker();
        }

        public void ExitScope()
        {
            _ = _entries.PopUntilMarker();
        }
    }
}

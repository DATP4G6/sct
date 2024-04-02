namespace Sct.Compiler
{
    public class Vtable
    {
        private readonly StackAdapter<VtableEntry> _entries = new StackAdapter<VtableEntry>();

        public bool AddEntry(string name, SctType type)
        {
            if (_entries.Any(x => x.Name == name))
            {
                return false;
            }
            _entries.Push(new VtableEntry(name, type));
            return true;
        }

        public SctType? Lookup(string name)
        {
            return _entries.Where(x => x.Name == name)
                    .FirstOrDefault()?.Type;
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

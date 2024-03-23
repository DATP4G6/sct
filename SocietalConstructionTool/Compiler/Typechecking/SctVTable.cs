namespace Sct.Compiler.Typechecking
{
    public class SctVTable
    {
        private readonly StackAdapter<(string name, SctType type)> _stack = new();
        public void Bind(string name, SctType type) => _stack.Push((name, type));
        public void EnterScope() => _stack.PushMarker();
        public void ExitScope() => _stack.PopUntilMarker();
        public SctType? Lookup(string name) => _stack.FirstOrDefault(x => x.name == name).type;
    }
}

namespace Sct.Compiler.Typechecking
{
    public readonly struct SctFunction
    {
        public SctType ReturnType { get; }
        public SctType[] ParameterTypes { get; }
        public SctFunction(SctType returnType, SctType[] parameterTypes)
        {
            (ReturnType, ParameterTypes) = (returnType, parameterTypes);
        }
    }
    public class SctFTable : ISctFTable
    {
        private readonly Dictionary<string, SctFunction> _baseScope;
        private readonly Queue<IDictionary<string, SctFunction>> _scopes;
        private Dictionary<string, SctFunction> _currentScope;
        private SctFTable(Dictionary<string, SctFunction> baseScope, Queue<IDictionary<string, SctFunction>> scopes)
        {
            _baseScope = baseScope;
            _currentScope = baseScope;
            _scopes = scopes;
        }
        public void LoadNextScope() => _currentScope = _scopes.Dequeue().Concat(_baseScope).ToDictionary();
        public void UnloadLastScope() => _currentScope = _baseScope;
        public SctFunction? Lookup(string name)
        {
            if (_currentScope.TryGetValue(name, out var function))
                return function;
            return null;
        }
        public override string ToString() => string.Join("\n", _currentScope.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        public class SctFTableBuilder
        {
            private readonly Queue<Dictionary<string, SctFunction>> _scopes = new();
            public SctFTableBuilder()
            {
                _scopes.Enqueue(new());
            }

            // public SctFTable Build() => new(_scopes.ElementAt(0), new(_scopes.Where((_, i) => i != 0)));
            public SctFTable Build() => new(_scopes.ElementAt(0), new(_scopes.Take(1..)));
            public SctFTableBuilder AddFunction(string name, SctType returnType, params SctType[] parameterTypes)
            {
                _scopes.Peek().Add(name, new(returnType, parameterTypes));
                return this;
            }
            public SctFTableBuilder EnterScope()
            {
                _scopes.Enqueue(new());
                return this;
            }
        }
    }
}

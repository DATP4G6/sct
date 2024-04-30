using System.Collections.Immutable;

namespace Sct.Compiler.Typechecker
{
    public class FTable
    {
        private readonly Dictionary<string, FunctionType> _ftable = new();
        public ImmutableDictionary<string, FunctionType> Functions => _ftable.ToImmutableDictionary();

        public FunctionType? GetFunctionType(string name) => _ftable.TryGetValue(name, out var functionType) ? functionType : default;

        public bool AddFunctionType(string name, FunctionType functionType) => _ftable.TryAdd(name, functionType);

        public bool Contains(string name) => _ftable.ContainsKey(name);
    }
}

namespace Sct.Compiler
{
    public class Ftable
    {
        private readonly Dictionary<string, FunctionType> _ftable = new Dictionary<string, FunctionType>();

        public FunctionType? GetFunctionType(string name)
        {
            return _ftable.TryGetValue(name, out var functionType) ? functionType : default;
        }

        public bool AddFunctionType(string name, FunctionType functionType)
        {
            return _ftable.TryAdd(name, functionType);
        }

        public bool Contains(string name)
        {
            return _ftable.ContainsKey(name);
        }
    }


}


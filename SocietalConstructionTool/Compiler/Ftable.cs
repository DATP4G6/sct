namespace Sct.Compiler
{
    public class Ftable
    {
        public Dictionary<string, FunctionType> ftable = new Dictionary<string, FunctionType>();

        public FunctionType GetFunctionType(string name)
        {
            return ftable.TryGetValue(name, out FunctionType functionType) ? functionType : null;
        }

        public bool AddFunctionType(string name, FunctionType functionType)
        {
            return ftable.TryAdd(name, functionType);
        }

        public bool Contains(string name)
        {
            return ftable.ContainsKey(name);
        }
    }


}


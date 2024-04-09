namespace Sct.Compiler.Typechecker
{
    public class ClassContent(string name)
    {
        public string Name { get; } = name;
        public FTable FTable { get; } = new();
        public HashSet<string> STable { get; } = new();
        public HashSet<string> DTable { get; } = new();
        public Dictionary<string, SctType> Fields { get; } = new();

        public bool AddFunction(string name, FunctionType functionType)
        {
            if (IDExists(name))
            {
                return false;
            }
            return FTable.AddFunctionType(name, functionType);
        }

        public bool AddState(string name)
        {
            if (IDExists(name))
            {
                return false;
            }
            _ = STable.Add(name);
            return true;
        }

        public bool AddDecorator(string name)
        {
            if (IDExists(name))
            {
                return false;
            }
            _ = DTable.Add(name);
            return true;
        }

        public bool AddField(string name, SctType type)
        {
            if (IDExists(name))
            {
                return false;
            }
            Fields.Add(name, type);
            return true;
        }

        public bool HasState(string name) => STable.Contains(name);

        public bool HasDecorator(string name) => DTable.Contains(name);

        public FunctionType? LookupFunctionType(string name) => FTable.GetFunctionType(name);

        private bool IDExists(string name) => FTable.Contains(name) || STable.Contains(name) || DTable.Contains(name);
    }
}

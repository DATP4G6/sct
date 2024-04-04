namespace Sct.Compiler.Typechecker
{
    public class ClassContent
    {
        public string Name { get; }
        public Ftable Ftable { get; }
        public List<string> STable { get; }
        public List<string> Dtable { get; }
        public Dictionary<string, SctType> Fields { get; }


        public ClassContent(string name)
        {
            Name = name;
            Fields = new Dictionary<string, SctType>();
            Ftable = new Ftable();
            STable = new List<string>();
            Dtable = new List<string>();
        }

        public bool AddFunction(string name, FunctionType functionType)
        {
            if (IDExists(name))
            {
                return false;
            }
            return Ftable.AddFunctionType(name, functionType);
        }

        public bool AddState(string name)
        {
            if (IDExists(name))
            {
                return false;
            }
            STable.Add(name);
            return true;
        }

        public bool AddDecorator(string name)
        {
            if (IDExists(name))
            {
                return false;
            }
            Dtable.Add(name);
            return true;
        }

        public string? LookupState(string name)
        {
            return STable.Contains(name) ? name : null;
        }

        public string? LookupDecorator(string name)
        {
            return Dtable.Contains(name) ? name : null;
        }

        public FunctionType? LookupFunctionType(string name)
        {
            return Ftable.GetFunctionType(name);
        }

        private bool IDExists(string name)
        {

            return Ftable.GetFunctionType(name) is not null || STable.Contains(name) || Dtable.Contains(name);
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
    }
}


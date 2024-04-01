namespace Sct.Compiler
{
    public class ClassContent
    {
        public Ftable Ftable { get; }
        public Vtable Vtable { get; }
        public List<string> STable { get; }
        public List<string> Dtable { get; }


        public ClassContent()
        {
            Ftable = new Ftable();
            STable = new List<string>();
            Dtable = new List<string>();
            Vtable = new Vtable();
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

        public bool AddVariable(string name, SctType type)
        {
            if (IDExists(name))
            {
                return false;
            }
            Vtable.AddEntry(name, type);
            return true;
        }

        public SctType LookupVariable(string name)
        {
            return Vtable.Lookup(name);
        }
        public string? LookupState(string name)
        {
            return STable.Contains(name) ? name : null;
        }

        public string? LookupDecorator(string name)
        {
            return Dtable.Contains(name) ? name : null;
        }

        public FunctionType LookupFunctionType(string name)
        {
            return Ftable.GetFunctionType(name);
        }

        private bool IDExists(string name)
        {

            return Ftable.GetFunctionType(name) is not null || STable.Contains(name) || Dtable.Contains(name);
        }
    }
}


namespace Sct.Compiler
{
    public class ClassContent
    {
        public Ftable Ftable { get; }
        public List<string> STable { get; }
        public List<string> Dtable { get; }

        public ClassContent()
        {
            Ftable = new Ftable();
            STable = new List<string>();
            Dtable = new List<string>();
        }

        public ClassContent(Ftable ftable)
        {
            Ftable = ftable;
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

        public string lookupState(string name)
        {
            return STable.Contains(name) ? name : null;
        }

        public string lookupDecorator(string name)
        {
            return Dtable.Contains(name) ? name : null;
        }

        public FunctionType LookupFunctionType(string name)
        {
            return Ftable.GetFunctionType(name);
        }

        public bool StateExists(string stateName)
        {
            return STable.Contains(stateName);
        }

        public bool DecoratorExists(string decoratorName)
        {
            return Dtable.Contains(decoratorName);
        }

        private bool IDExists(string name)
        {
            return Ftable.GetFunctionType(name) is not null || STable.Contains(name) || Dtable.Contains(name);
        }


    }
}


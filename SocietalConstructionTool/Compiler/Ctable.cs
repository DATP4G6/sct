namespace Sct.Compiler
{
    public class Ctable
    {
        readonly Dictionary<string, ClassContent> _entries = new Dictionary<string, ClassContent>();
        private readonly TypeTable _typeTable = new TypeTable();

        public Ctable()
        {
            ClassContent global = new ClassContent();
            List<SctType> parameterList = [_typeTable.GetType("Predicate")!];
            global.AddFunction("count", new FunctionType(_typeTable.GetType("int")!, parameterList));
            _entries.Add("global", new ClassContent());
        }

        public void AddClass(string className, Ftable ftable)
        {
            _entries.Add(className, new ClassContent(ftable));
        }

        public FunctionType GetFunctionType(string className, string functionName)
        {
            return _entries[className].GetFunctionType(functionName);
        }

        public void AddFunction(string className, string functionName, FunctionType functionType)
        {
            _entries[className].AddFunction(functionName, functionType);
        }

    }
}


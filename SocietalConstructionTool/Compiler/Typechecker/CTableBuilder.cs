namespace Sct.Compiler.Typechecker
{
    public class CTableBuilder
    {
        private KeyValuePair<string, ClassContent> _currentClass;
        private readonly KeyValuePair<string, ClassContent> _globalClass;
        private readonly Dictionary<string, ClassContent> _classes = new();

        public CTableBuilder()
        {
            _globalClass = new KeyValuePair<string, ClassContent>("Global", new ClassContent("Global"));
            _currentClass = _globalClass;
            _ = TryAddFunction("count", new FunctionType(TypeTable.Int, [TypeTable.Predicate]));
            _ = TryAddFunction("exists", new FunctionType(TypeTable.Int, [TypeTable.Predicate]));
            _ = TryAddFunction("rand", new FunctionType(TypeTable.Float, []));
            _ = TryAddFunction("seed", new FunctionType(TypeTable.Void, [TypeTable.Int]));
        }

        public CTable BuildCtable() => new(_classes, _globalClass.Value);

        public bool TryStartClass(string className)
        {
            if (_classes.ContainsKey(className))
            {
                return false;
            }
            _currentClass = new KeyValuePair<string, ClassContent>(className, new ClassContent(className));
            return true;
        }

        public void FinishClass()
        {
            _classes.Add(_currentClass.Key, _currentClass.Value);
            _currentClass = _globalClass;
        }

        public bool TryAddField(string name, SctType type)
        {
            if (_currentClass.Value.Fields.ContainsKey(name))
            {
                return false;
            }
            return _currentClass.Value.AddField(name, type);
        }

        public bool TryAddFunction(string functionName, FunctionType functionType)
        {
            if (IDExistsGlobal(functionName))
            {
                return false;
            }
            return _currentClass.Value.AddFunction(functionName, functionType);
        }

        public bool TryAddState(string name)
        {
            if (IDExistsGlobal(name))
            {
                return false;
            }
            return _currentClass.Value.AddState(name);
        }

        public bool TryAddDecorator(string name)
        {
            if (IDExistsGlobal(name))
            {
                return false;
            }
            return _currentClass.Value.AddDecorator(name);
        }

        private bool IDExistsGlobal(string name) => _globalClass.Value?.LookupFunctionType(name) is not null;
    }
}

using Sct.Compiler;

namespace Sct
{

    public class CtableBuilder
    {

        private KeyValuePair<string, ClassContent> _currentClass;
        private readonly KeyValuePair<string, ClassContent> _globalClass;

        private readonly Dictionary<string, ClassContent> _classes = new Dictionary<string, ClassContent>();

        public CtableBuilder()
        {
            _globalClass = new KeyValuePair<string, ClassContent>("Global", new ClassContent("Global"));
            _currentClass = _globalClass;
            _ = AddFunction("count", new FunctionType(TypeTable.Int, [TypeTable.Predicate]));
            _ = AddFunction("exists", new FunctionType(TypeTable.Int, [TypeTable.Predicate]));
            _ = AddFunction("rand", new FunctionType(TypeTable.Float, []));
            _ = AddFunction("seed", new FunctionType(TypeTable.Float, [TypeTable.Int]));
        }

        public Ctable BuildCtable()
        {
            return new Ctable(_classes, _globalClass.Value);
        }

        public bool StartClass(string className)
        {
            if (_classes.ContainsKey(className))
            {
                return false;
            }
            _currentClass = new KeyValuePair<string, ClassContent>(className, new ClassContent(className));
            return true;
        }

        public bool FinishClass()
        {
            _classes.Add(_currentClass.Key, _currentClass.Value);
            _currentClass = _globalClass;

            return _classes.ContainsKey(_currentClass.Key);
        }

        public bool AddField(string name, SctType type)
        {
            if (_currentClass.Value.Fields.ContainsKey(name))
            {
                return false;
            }
            return _currentClass.Value.AddField(name, type);
        }

        public bool AddFunction(string functionName, FunctionType functionType)
        {
            if (IDExistsGlobal(functionName))
            {
                return false;
            }
            return _currentClass.Value.AddFunction(functionName, functionType);
        }

        public bool AddState(string name)
        {
            if (IDExistsGlobal(name))
            {
                return false;
            }
            return _currentClass.Value.AddState(name);
        }

        public bool AddDecorator(string name)
        {
            if (IDExistsGlobal(name))
            {
                return false;
            }
            return _currentClass.Value.AddDecorator(name);
        }

        private bool IDExistsGlobal(string name)
        {
            if (_globalClass.Value?.LookupFunctionType(name) is not null)
            {
                return true;
            }
            return false;
        }
    }
}

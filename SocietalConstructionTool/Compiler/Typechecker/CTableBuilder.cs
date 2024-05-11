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
            _ = TryAddFunction("count", new FunctionType(Syntax.SctType.Int, [Syntax.SctType.Predicate]));
            _ = TryAddFunction("exists", new FunctionType(Syntax.SctType.Int, [Syntax.SctType.Predicate]));
            _ = TryAddFunction("rand", new FunctionType(Syntax.SctType.Float, []));
            _ = TryAddFunction("seed", new FunctionType(Syntax.SctType.Void, [Syntax.SctType.Int]));
        }

        public (CTable cTable, List<CompilerError> errors) BuildCtable()
        {
            CTable cTable = new(_classes, _globalClass.Value);
            var errors = new List<CompilerError>();

            var setupType = cTable.GlobalClass.LookupFunctionType("setup");
            if (setupType is null)
            {
                errors.Add(new CompilerError("No setup function found"));
            }
            else if (setupType.ReturnType != Syntax.SctType.Void || setupType.ParameterTypes.Count != 0)
            {
                errors.Add(new CompilerError("Setup function must return void and take no arguments"));
            }

            return (cTable, errors);
        }

        public bool TryStartClass(string className)
        {
            if (IDExistsGlobal(className))
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

        public bool TryAddField(string name, Syntax.SctType type)
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

        private bool IDExistsGlobal(string name) => _globalClass.Value?.LookupFunctionType(name) is not null || _classes.ContainsKey(name);
    }
}

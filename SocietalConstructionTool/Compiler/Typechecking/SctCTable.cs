namespace Sct.Compiler.Typechecking
{
    public class SctCTable
    {
        private readonly Dictionary<string, Dictionary<string, SctType>> _fieldsTable;
        private readonly Dictionary<string, IEnumerable<string>> _statesTable;
        public bool ContainsClass(string className) => _fieldsTable.ContainsKey(className) && _statesTable.ContainsKey(className);
        public Dictionary<string, SctType> LookupFields(string className) => _fieldsTable[className];
        public IEnumerable<string> LookupStates(string className) => _statesTable[className];
        private SctCTable(Dictionary<string, Dictionary<string, SctType>> fieldsTable, Dictionary<string, IEnumerable<string>> statesTable)
        {
            _fieldsTable = fieldsTable;
            _statesTable = statesTable;
        }
        public override string ToString() => string.Join("\n", _fieldsTable.Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}\n\t{string.Join(", ", _statesTable[kvp.Key])}"));
        public class SctCTableBuilder
        {
            private readonly Dictionary<string, Dictionary<string, SctType>> _fieldsTable = new();
            private readonly Dictionary<string, HashSet<string>> _statesTable = new();

            private Dictionary<string, SctType> _currentFields = new();
            private HashSet<string> _currentStates = new();
            public SctCTable Build() => new(_fieldsTable, _statesTable.Select(kvp => (kvp.Key, kvp.Value.AsEnumerable())).ToDictionary());
            public bool FinishClass(string name)
            {
                if (_currentFields is null || _currentStates is null)
                {
                    throw new InvalidOperationException("No class selected");
                }
                var status = _fieldsTable.TryAdd(name, _currentFields) && _statesTable.TryAdd(name, _currentStates);
                _currentFields = new();
                _currentStates = new();
                return status;
            }
            public bool AddField(string name, SctType type)
            {
                if (_currentFields is null)
                {
                    throw new InvalidOperationException("No class selected");
                }
                return _currentFields.TryAdd(name, type);
            }
            public bool AddState(string name)
            {
                if (_currentStates is null)
                {
                    throw new InvalidOperationException("No class selected");
                }
                return _currentStates.Add(name);
            }
        }
    }
}

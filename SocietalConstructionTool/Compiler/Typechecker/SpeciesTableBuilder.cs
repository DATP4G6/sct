namespace Sct.Compiler.Typechecker
{
    public class CTableBuilder
    {
        private KeyValuePair<string, SpeciesContent> _currentSpecies;
        private readonly KeyValuePair<string, SpeciesContent> _globalContent;
        private readonly Dictionary<string, SpeciesContent> _species = new();

        public CTableBuilder()
        {
            _globalContent = new KeyValuePair<string, SpeciesContent>("Global", new SpeciesContent("Global"));
            _currentSpecies = _globalContent;
            _ = TryAddFunction("count", new FunctionType(Syntax.SctType.Int, [Syntax.SctType.Predicate]));
            _ = TryAddFunction("exists", new FunctionType(Syntax.SctType.Int, [Syntax.SctType.Predicate]));
            _ = TryAddFunction("rand", new FunctionType(Syntax.SctType.Float, []));
            _ = TryAddFunction("seed", new FunctionType(Syntax.SctType.Void, [Syntax.SctType.Int]));
        }

        public (CTable cTable, List<CompilerError> errors) BuildCtable()
        {
            CTable cTable = new(_species, _globalContent.Value);
            var errors = new List<CompilerError>();

            var setupType = cTable.GlobalContent.LookupFunctionType("Setup");
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

        public bool TryStartSpecies(string speciesName)
        {
            if (IDExistsGlobal(speciesName))
            {
                return false;
            }
            _currentSpecies = new KeyValuePair<string, SpeciesContent>(speciesName, new SpeciesContent(speciesName));
            return true;
        }

        public void FinishSpecies()
        {
            _species.Add(_currentSpecies.Key, _currentSpecies.Value);
            _currentSpecies = _globalContent;
        }

        public bool TryAddField(string name, Syntax.SctType type)
        {
            if (_currentSpecies.Value.Fields.ContainsKey(name))
            {
                return false;
            }
            return _currentSpecies.Value.AddField(name, type);
        }

        public bool TryAddFunction(string functionName, FunctionType functionType)
        {
            if (IDExistsGlobal(functionName))
            {
                return false;
            }
            return _currentSpecies.Value.AddFunction(functionName, functionType);
        }

        public bool TryAddState(string name)
        {
            if (IDExistsGlobal(name))
            {
                return false;
            }
            return _currentSpecies.Value.AddState(name);
        }

        public bool TryAddDecorator(string name)
        {
            if (IDExistsGlobal(name))
            {
                return false;
            }
            return _currentSpecies.Value.AddDecorator(name);
        }

        private bool IDExistsGlobal(string name) => _globalContent.Value?.LookupFunctionType(name) is not null || _species.ContainsKey(name);
    }
}

using Sct.Compiler;

namespace Sct
{

    public class CtableBuilder
    {

        private KeyValuePair<string, ClassContent> _currentClass;
        private readonly KeyValuePair<string, ClassContent> _globalClass;

        private readonly Dictionary<string, ClassContent> _classes = new Dictionary<string, ClassContent>();

        public Ctable BuildCtable()
        {
            _classes.Add(_globalClass.Key, _globalClass.Value);
            return new Ctable(_classes);
        }

        public bool StartClass(string className)
        {
            // TODO: Make checks for existing class
            _currentClass = new KeyValuePair<string, ClassContent>(className, new ClassContent());
            return false;
        }

        public bool FinishClass()
        {
            _classes.Add(_currentClass.Key, _currentClass.Value);
            _currentClass = _globalClass;

            return false;
        }

        public bool AddFunction(string functionName, FunctionType functionType)
        {
            _currentClass.Value.AddFunction(functionName, functionType);
            return false;
        }

        public bool AddState()
        {
            return false;

        }

        public bool AddDecorator()
        {
            return false;

        }
    }
}

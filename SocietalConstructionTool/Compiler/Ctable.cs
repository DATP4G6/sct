namespace Sct.Compiler
{
    public class Ctable
    {
        readonly Dictionary<string, ClassContent> _classes = new Dictionary<string, ClassContent>();
        readonly ClassContent _globalClass = new ClassContent();
        public Ctable(Dictionary<string, ClassContent> classes, ClassContent globalClass)
        {
            _classes = classes;
            _globalClass = globalClass;
        }

        public FunctionType GetFunctionType(string className, string functionName)
        {
            if (_classes.TryGetValue(className, out ClassContent classContent)) {
                return classContent.Ftable.GetFunctionType(functionName);
            } else {
                return _globalClass.Ftable.GetFunctionType(functionName);
            }
        }

        public bool StateExists(string className, string stateName)
        {
            return _classes.TryGetValue(className, out ClassContent classContent) && classContent.STable.Contains(stateName);
        }

        public bool DecoratorExists(string className, string decoratorName)
        {
            return _classes.TryGetValue(className, out ClassContent classContent) && classContent.Dtable.Contains(decoratorName);
        }
    }
}


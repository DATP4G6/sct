namespace Sct.Compiler
{
    public class Ctable
    {
        readonly Dictionary<string, ClassContent> _classes = new Dictionary<string, ClassContent>();
        public Ctable(Dictionary<string, ClassContent> classes)
        {
            _classes = classes;
        }
        public FunctionType GetFunctionType(string className, string functionName)
        {
            return _classes.TryGetValue(className, out ClassContent classContent) ? classContent.Ftable.GetFunctionType(functionName) : null;
        }

        public bool stateExists(string className, string stateName)
        {
            return _classes.TryGetValue(className, out ClassContent classContent) && classContent.STable.Contains(stateName);
        }
    }
}


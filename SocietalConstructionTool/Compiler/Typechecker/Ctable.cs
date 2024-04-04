namespace Sct.Compiler.Typechecker
{
    public class Ctable
    {
        readonly Dictionary<string, ClassContent> _classes = new Dictionary<string, ClassContent>();
        readonly ClassContent _globalClass;
        public Ctable(Dictionary<string, ClassContent> classes, ClassContent globalClass)
        {
            _classes = classes;
            _globalClass = globalClass;
        }

        public ClassContent GetClassContent(string className)
        {
            return _classes[className];
        }

        public ClassContent GetGlobalContent()
        {
            return _globalClass;
        }
    }
}

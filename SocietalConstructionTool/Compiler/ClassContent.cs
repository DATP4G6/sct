using System.Collections;

namespace Sct.Compiler
{
    public class ClassContent
    {
        public Ftable Ftable { get; }

        public ClassContent()
        {
            Ftable = new Ftable();
        }

        public ClassContent(Ftable ftable)
        {
            Ftable = ftable;
        }

        public FunctionType GetFunctionType(string name)
        {
            return Ftable.GetFunctionType(name);
        }

        public void AddFunction(string name, FunctionType functionType)
        {
            Ftable.AddFunctionType(name, functionType);
        }
    }


}


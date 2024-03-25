namespace Sct.Compiler
{
    public class Ftable
    {
        public Dictionary<string, FunctionType> ftable = new Dictionary<string, FunctionType>();

        public FunctionType GetFunctionType(string name)
        {
            return ftable[name];
        }

        public void AddFunctionType(string name, FunctionType functionType)
        {
            ftable.Add(name, functionType);
        }
    }


}


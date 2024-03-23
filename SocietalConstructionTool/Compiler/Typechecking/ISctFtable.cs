namespace Sct.Compiler.Typechecking
{
    public interface ISctFTable
    {
        public void LoadNextScope();
        public void UnloadLastScope();
        public SctFunction? Lookup(string name);
    }
}

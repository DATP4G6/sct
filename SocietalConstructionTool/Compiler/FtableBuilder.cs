using Sct.Compiler;

public class FtableBuilder
{
    private readonly Ftable _ftable = new Ftable();

    public void AddFunctionType(string name, FunctionType functionType)
    {
        _ftable.AddFunctionType(name, functionType);
    }

    public Ftable GetFtable()
    {
        return _ftable;
    }
}

namespace Sct.Runtime.Trace
{
    public interface IOutputLogger
    {
        public void OnTick(IRuntimeContext context);
        public void OnExit() { }
    }
}

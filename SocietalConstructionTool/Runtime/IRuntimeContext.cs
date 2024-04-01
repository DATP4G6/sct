namespace Sct.Runtime
{
    public interface IRuntimeContext
    {
        public IAgentHandler AgentHandler { get; }
        public IQueryHandler QueryHandler { get; }
        public bool ShouldExit { get; }
        public void ExitRuntime();
        public IRuntimeContext GetNextContext();
    }
}

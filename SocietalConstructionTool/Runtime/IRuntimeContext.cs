namespace Sct.Runtime
{
    public interface IRuntimeContext
    {
        public IAgentHandler AgentHandler { get; }
        public IQueryHandler QueryHandler { get; }
        public void Exit();
    }
}

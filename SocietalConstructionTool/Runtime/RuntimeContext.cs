namespace Sct.Runtime
{
    public class RuntimeContext : IRuntimeContext
    {
        public IAgentHandler AgentHandler { get; }
        public IQueryHandler QueryHandler { get; }
        public bool ShouldExit { get; private set; }
        public RuntimeContext(IAgentHandler agentHandler, IQueryHandler queryHandler)
        {
            AgentHandler = agentHandler;
            QueryHandler = queryHandler;
            ShouldExit = false;
        }

        public void Exit()
        {
            throw new NotImplementedException("Exit not implemented.");
        }
    }
}

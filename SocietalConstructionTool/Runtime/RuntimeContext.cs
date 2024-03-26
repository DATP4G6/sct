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

        public RuntimeContext()
        {
            AgentHandler = new AgentHandler();
            QueryHandler = new QueryHandler([]);
            ShouldExit = false;
        }

        public void ExitRuntime()
        {
            throw new NotImplementedException("Exit not implemented.");
        }
    }
}

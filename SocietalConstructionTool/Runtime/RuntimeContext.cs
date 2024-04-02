using Sct.Runtime.Trace;

namespace Sct.Runtime
{
    public class RuntimeContext(IAgentHandler agentHandler, IQueryHandler queryHandler, IOutputLogger? outputLogger) : IRuntimeContext
    {
        public IAgentHandler AgentHandler { get; } = agentHandler;
        public IQueryHandler QueryHandler { get; } = queryHandler;
        public IOutputLogger? OutputLogger { get; } = outputLogger;

        public bool ShouldExit { get; private set; } = false;

        public RuntimeContext() : this(new AgentHandler(), new QueryHandler([]), null)
        { }

        public void ExitRuntime()
        {
            ShouldExit = true;
        }

        public IRuntimeContext GetNextContext() => RuntimeContextFactory.CreateNext(this);
    }
}

using Sct.Runtime.Trace;

namespace Sct.Runtime
{
    public interface IRuntimeContext
    {
        public IAgentHandler AgentHandler { get; }
        public IQueryHandler QueryHandler { get; }
        public IOutputLogger? OutputLogger { get; }
        public bool ShouldExit { get; }
        public void ExitRuntime();
        public IRuntimeContext GetNextContext();

        public void OnTick() => OutputLogger?.OnTick(this);
        public void OnExit() => OutputLogger?.OnExit();
    }
}

namespace Sct.Runtime
{
    public class Runtime() : IRuntime
    {
        public void Run(IRuntimeContext initialContext)
        {
            IRuntimeContext prevCtx = initialContext;
            while (!prevCtx.ShouldExit && prevCtx.AgentHandler.Agents.Any())
            {
                prevCtx.OnTick();
                IRuntimeContext nextCtx = prevCtx.GetNextContext();
                _ = Parallel.ForEach(prevCtx.AgentHandler.Agents, agent => agent.Update(nextCtx));
                prevCtx = nextCtx;
            }

            prevCtx.OnTick();
            prevCtx.OnExit();
        }
    }
}

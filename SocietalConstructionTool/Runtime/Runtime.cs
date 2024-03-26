namespace Sct.Runtime
{
    public class Runtime() : IRuntime
    {
        public void Run(IRuntimeContext initialContext)
        {
            IRuntimeContext prevCtx = initialContext;
            while (!prevCtx.ShouldExit)
            {
                IRuntimeContext nextCtx = RuntimeContextFactory.CreateNext(prevCtx);
                foreach (var agent in prevCtx.AgentHandler.Agents)
                {
                    agent.Update(nextCtx);
                }
                prevCtx = nextCtx;
            }
        }
    }
}

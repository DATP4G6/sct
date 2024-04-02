namespace Sct.Runtime
{
    public class Runtime() : IRuntime
    {
        public void Run(IRuntimeContext initialContext)
        {
            IRuntimeContext prevCtx = initialContext;
            while (!prevCtx.ShouldExit)
            {
                prevCtx.OnTick();
                IRuntimeContext nextCtx = prevCtx.GetNextContext();
                foreach (var agent in prevCtx.AgentHandler.Agents)
                {
                    agent.Update(nextCtx);
                }
                prevCtx = nextCtx;
            }

            prevCtx.OnExit();
        }
    }
}

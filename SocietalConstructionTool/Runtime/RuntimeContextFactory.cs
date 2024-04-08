using Sct.Runtime.Trace;

namespace Sct.Runtime
{
    public class RuntimeContextFactory
    {
        public static IRuntimeContext Create() => Create(null);

        public static IRuntimeContext Create(IOutputLogger? logger) => new RuntimeContext(new AgentHandler(), new QueryHandler([]), logger);

        /// <summary>
        /// Create the next RuntimeContext based on the current context.
        ///
        /// Agents added to agent handler will be passed to the new query handler, and agent handler is reset
        /// </summary>
        /// <param name="ctx">The previous context to update from</param>
        /// <returns></returns>
        public static IRuntimeContext CreateNext(IRuntimeContext ctx)
        {
            return new RuntimeContext(new AgentHandler(), new QueryHandler(ctx.AgentHandler.Agents), ctx.OutputLogger);
        }
    }
}

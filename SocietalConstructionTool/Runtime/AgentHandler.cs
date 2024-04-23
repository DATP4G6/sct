using System.Collections.Concurrent;

namespace Sct.Runtime
{
    public class AgentHandler : IAgentHandler
    {
        private readonly ConcurrentBag<BaseAgent> _agents = [];
        public IEnumerable<BaseAgent> Agents => _agents;
        public void CreateAgent(BaseAgent agent) => _agents.Add(agent);
    }
}

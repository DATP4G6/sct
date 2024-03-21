namespace Sct.Runtime
{
    public class AgentHandler : IAgentHandler
    {
        private readonly List<BaseAgent> _agents = [];
        public IEnumerable<BaseAgent> Agents => _agents.AsReadOnly();
        public void CreateAgent(BaseAgent agent)
        {
            _agents.Add(agent);
        }
    }
}

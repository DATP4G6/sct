namespace Sct.Runtime
{
    public interface IAgentHandler
    {
        public IEnumerable<BaseAgent> Agents { get; }
        public void CreateAgent(BaseAgent agent);
    }
}

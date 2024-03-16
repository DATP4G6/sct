namespace Sct.Runtime
{
    public interface IAgentHandler
    {
        public void CreateAgent<T>(T agent) where T : BaseAgent;
    }
}

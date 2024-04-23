using Sct.Runtime;

namespace Sct.Extensions
{
    public static class AgentExtensions
    {
        public static IEnumerable<BaseAgent> DeterministicOrder(this IEnumerable<BaseAgent> agents) =>
            agents.OrderBy(a => a, new BaseAgent.Comparer());
    }
}

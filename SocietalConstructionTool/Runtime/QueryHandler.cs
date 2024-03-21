using System.Linq;

namespace Sct.Runtime
{
    public class QueryHandler : IQueryHandler
    {
        private readonly IEnumerable<BaseAgent> _agents;
        public QueryHandler(IEnumerable<BaseAgent> agents)
        {
            _agents = agents;
        }

        private IEnumerable<BaseAgent> Filter(IQueryPredicate<BaseAgent> predicate) => _agents.Where(a => predicate.Test(a));

        public int Count(IQueryPredicate<BaseAgent> predicate) => Filter(predicate).Count();

        public bool Exists(IQueryPredicate<BaseAgent> predicate) => Count(predicate) > 0;
    }
}

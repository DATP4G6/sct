namespace Sct.Runtime
{
    public class QueryHandler(IEnumerable<BaseAgent> agents) : IQueryHandler
    {
        public IEnumerable<BaseAgent> Filter(IQueryPredicate predicate) => agents.Where(predicate.Test);
    }
}

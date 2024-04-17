namespace Sct.Runtime
{
    public class QueryHandler(IEnumerable<BaseAgent> agents) : IQueryHandler
    {
        private IEnumerable<BaseAgent> Filter(IQueryPredicate predicate) => agents.Where(predicate.Test);

        public int Count(IQueryPredicate predicate) => Filter(predicate).Count();

        public bool Exists(IQueryPredicate predicate) => Filter(predicate).Any();
    }
}

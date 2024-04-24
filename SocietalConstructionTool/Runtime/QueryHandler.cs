namespace Sct.Runtime
{
    public class QueryHandler(IEnumerable<BaseAgent> agents) : IQueryHandler
    {
        private IEnumerable<BaseAgent> Filter(IQueryPredicate predicate) => agents.Where(predicate.Test);

        public long Count(IRuntimeContext ctx, IQueryPredicate predicate) => Filter(predicate).Count();
        public long Exists(IRuntimeContext ctx, IQueryPredicate predicate) => Filter(predicate).Any() ? 1 : 0;
    }
}

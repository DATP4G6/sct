namespace Sct.Runtime
{
    public class QueryHandler(IEnumerable<BaseAgent> agents) : IQueryHandler
    {
        public IEnumerable<BaseAgent> Agents => agents;
        public IEnumerable<BaseAgent> Filter(IQueryPredicate predicate) => agents.Where(predicate.Test);

        public int Count(IRuntimeContext ctx, IQueryPredicate predicate) => Filter(predicate).Count();
        public bool Exists(IRuntimeContext ctx, IQueryPredicate predicate) => Filter(predicate).Any();
    }
}

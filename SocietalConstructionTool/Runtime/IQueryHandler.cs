namespace Sct.Runtime
{
    public interface IQueryHandler
    {
        public IEnumerable<BaseAgent> Filter(IQueryPredicate predicate);
        public long Count(IRuntimeContext ctx, IQueryPredicate predicate) => Filter(predicate).Count();
        public long Exists(IRuntimeContext ctx, IQueryPredicate predicate) => Filter(predicate).Any() ? 1 : 0;
    }
}

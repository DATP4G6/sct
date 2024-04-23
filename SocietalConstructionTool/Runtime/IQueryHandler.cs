namespace Sct.Runtime
{
    public interface IQueryHandler
    {
        public IEnumerable<BaseAgent> Agents { get; }
        public int Count(IRuntimeContext ctx, IQueryPredicate predicate);
        public bool Exists(IRuntimeContext ctx, IQueryPredicate predicate);
    }
}

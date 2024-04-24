namespace Sct.Runtime
{
    public interface IQueryHandler
    {
        public long Count(IRuntimeContext ctx, IQueryPredicate predicate);
        public long Exists(IRuntimeContext ctx, IQueryPredicate predicate);
    }
}

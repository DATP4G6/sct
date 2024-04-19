namespace Sct.Runtime
{
    public interface IQueryHandler
    {
        public int Count(IRuntimeContext ctx, IQueryPredicate predicate);
        public bool Exists(IRuntimeContext ctx, IQueryPredicate predicate);
    }
}

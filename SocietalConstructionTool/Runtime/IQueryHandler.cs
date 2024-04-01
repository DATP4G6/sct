namespace Sct.Runtime
{
    public interface IQueryHandler
    {
        public int Count(IQueryPredicate predicate);
        public bool Exists(IQueryPredicate predicate);
    }
}

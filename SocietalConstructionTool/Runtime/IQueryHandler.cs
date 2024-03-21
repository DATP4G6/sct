namespace Sct.Runtime
{
    public interface IQueryHandler
    {
        public int Count(IQueryPredicate<BaseAgent> predicate);
        public bool Exists(IQueryPredicate<BaseAgent> predicate);
    }
}

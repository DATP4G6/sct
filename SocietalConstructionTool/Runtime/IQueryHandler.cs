namespace Sct.Runtime
{
    public interface IQueryHandler
    {
        public int Count(IQueryPredicate<BaseAgent> predicate);
        public int Exists(IQueryPredicate<BaseAgent> predicate);
    }
}

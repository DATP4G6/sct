namespace Sct.Runtime
{
    public interface IQueryPredicate
    {
        /// <summary>
        /// Tests the predicate against the given agent.
        /// </summary>
        public bool Test(BaseAgent agent);
    }
}

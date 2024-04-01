namespace Sct.Runtime
{
    public interface IQueryPredicate
    {
        /// <summary>
        /// Name of the class this predicate targets
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// The state that the predicate should check.
        /// Can be null, as wildcards are allowed.
        ///
        /// Note that for agent creation, this cannot be null.
        /// </summary>
        public string? State { get; }

        /// <summary>
        /// The fields that the predicate should check.
        /// </summary>
        IDictionary<string, dynamic> Fields { get; }

        /// <summary>
        /// Tests the predicate against the given agent.
        /// </summary>
        public bool Test(BaseAgent agent);
    }
}

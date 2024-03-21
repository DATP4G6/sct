namespace Sct.Runtime
{
    public class QueryPredicate<T> : IQueryPredicate<T> where T : BaseAgent
    {
        public string? State { get; }

        public IDictionary<string, dynamic> Fields { get; }

        public QueryPredicate(string? state, IDictionary<string, dynamic> fields)
        {
            State = state;
            Fields = fields;
        }

        public bool Test(BaseAgent agent)
        {
            // Match the agent type
            if (agent is not T)
            {
                return false;
            }
            // Match the state
            if (State != null && agent.State != State)
            {
                return false;
            }
            // Match the fields
            return Fields.All(pair => agent.Fields[pair.Key] == pair.Value);
        }
    }
}

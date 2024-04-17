namespace Sct.Runtime
{
    public class QueryPredicate(string className, string? state, IDictionary<string, dynamic> fields) : IQueryPredicate
    {
        public bool Test(BaseAgent agent)
        {
            // Match the class name
            if (agent.GetType().Name != className)
            {
                return false;
            }
            // Match the state
            // If state is null, it's a wildcard
            if (state != null && agent.State != state)
            {
                return false;
            }
            // Match the fields
            return fields.All(pair => agent.Fields[pair.Key] == pair.Value);
        }
    }
}

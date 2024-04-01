namespace Sct.Runtime
{
    public class QueryPredicate : IQueryPredicate
    {
        public string ClassName { get; }
        public string? State { get; }

        public IDictionary<string, dynamic> Fields { get; }

        public QueryPredicate(string className, string? state, IDictionary<string, dynamic> fields)
        {
            ClassName = className;
            State = state;
            Fields = fields;
        }

        public bool Test(BaseAgent agent)
        {
            // Match the class name
            if (agent.GetType().Name != ClassName)
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

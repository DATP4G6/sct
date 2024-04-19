namespace Sct.Runtime
{
    public class QueryPredicate(string className, string? state, IDictionary<string, dynamic> fields) : IQueryPredicate
    {

        public string ClassName => className;
        public string? State => state;
        public IDictionary<string, dynamic> Fields => fields;

        public bool Test(BaseAgent agent)
        {
            // Match the class name
            if (agent.GetType().Name != ClassName)
            {
                return false;
            }
            // Match the state
            // If state is null, it's a wildcard
            if (state != null && agent.State != State)
            {
                return false;
            }
            // Match the fields
            return Fields.All(pair => agent.Fields[pair.Key] == pair.Value);
        }

        public static bool operator ==(QueryPredicate a, QueryPredicate b)
        {
            if (a.ClassName != b.ClassName)
                return false;
            if (a.State != b.State)
                return false;
            return a.Fields.All(pair => b.Fields[pair.Key] == pair.Value) && b.Fields.All(pair => a.Fields[pair.Key] == pair.Value);
        }

        public static bool operator !=(QueryPredicate a, QueryPredicate b)
        {
            return !(a == b);
        }
    }
}

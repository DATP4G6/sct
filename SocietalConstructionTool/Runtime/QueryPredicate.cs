namespace Sct.Runtime
{
    public class QueryPredicate(string speciesName, string? state, IDictionary<string, dynamic> fields) : IQueryPredicate
    {

        public string SpeciesName => speciesName;
        public string? State => state;
        public IDictionary<string, dynamic> Fields => fields;

        public bool Test(BaseAgent agent)
        {
            // Match the species name
            if (agent.GetType().Name != SpeciesName)
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
            if (a.SpeciesName != b.SpeciesName)
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

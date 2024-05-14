using Sct.Runtime.Trace;

namespace Sct.Runtime
{
    public static class Stdlib
    {
        private static Random s_random = new();
        public static double Rand(IRuntimeContext _)
        {
            return s_random.NextDouble();
        }

        public static void Seed(IRuntimeContext _, long seed)
        {
            s_random = new Random((int)seed);
        }

        public static void PrintPredicate(IRuntimeContext ctx, IQueryPredicate predicate)
        {
            var matches = ctx.QueryHandler.Filter(predicate);
            foreach (var agent in matches)
            {
                Console.WriteLine(new AgentDemangler(agent));
            }
        }

        public static void PrintPredicateCount(IRuntimeContext ctx, IQueryPredicate predicate)
        {
            var matches = ctx.QueryHandler.Filter(predicate);
            Console.WriteLine($"Agents matching predicate {predicate}: {matches.Count()}");
        }
    }
}

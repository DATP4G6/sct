using System.Text.Json;

using Sct.Extensions;

namespace Sct.Runtime
{
    public class BaseGlobalClass // Statically implements IRandom
    {
        private static Random s_random = new();
        public static void Seed(IRuntimeContext? _, int seed) => s_random = new(seed);
        public static double Rand(IRuntimeContext? _) => s_random.NextDouble();
        public static int RandInt(IRuntimeContext? _) => s_random.Next();
        public static void PrintPredicate(IRuntimeContext ctx, IQueryPredicate predicate)
        {
            var matches = ctx.QueryHandler.Agents.Where(predicate.Test).DeterministicOrder();
            Console.WriteLine($"Agents matching predicate {predicate}:");
            Console.WriteLine(JsonSerializer.Serialize(matches) + '\n');
        }
        public static void PrintPredicateCount(IRuntimeContext ctx, IQueryPredicate predicate)
        {
            var matches = ctx.QueryHandler.Agents.Where(predicate.Test).DeterministicOrder();
            Console.WriteLine($"Agents matching predicate {predicate}: {matches.Count()}");
        }
    }
}

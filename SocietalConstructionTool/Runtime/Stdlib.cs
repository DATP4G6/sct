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
    }
}

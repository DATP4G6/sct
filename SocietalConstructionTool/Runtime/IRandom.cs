namespace Sct.Runtime
{
    public interface IRandom
    {
        public void Seed(IRuntimeContext? _, int seed);
        public double Rand(IRuntimeContext? _);
        public int RandInt(IRuntimeContext? _);
    }
}

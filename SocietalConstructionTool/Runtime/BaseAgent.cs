namespace Sct.Runtime
{
    public abstract class BaseAgent : ICloneable, IRandom
    {
        public string State { get; set; }
        public IDictionary<string, dynamic> Fields { get; set; }
        private Random _random;

        // This makes it so that the class name shows up in serialization
        public string ClassName { get; }
        private double _comparisonId;

        public BaseAgent(string state, IDictionary<string, dynamic> fields, int seed)
        {
            State = state;
            Fields = fields.ToDictionary(entry => entry.Key, entry => entry.Value);
            ClassName = GetType().Name;
            _random = new Random(seed);
            _comparisonId = Rand(null);
        }

        public abstract void Update(IRuntimeContext ctx);

        public static string EnterMethodName => nameof(Enter);
        protected void Enter(IRuntimeContext ctx, string state)
        {
            BaseAgent a = (BaseAgent)Clone();

            // Set values specific to the clone
            a._random = new(_random.Next());
            a._comparisonId = a.Rand(null);
            a.State = state;
            ctx.AgentHandler.CreateAgent(a);
        }

        public object Clone()
        {
            BaseAgent a = (BaseAgent)MemberwiseClone();
            a.Fields = new Dictionary<string, dynamic>(Fields);
            return a;
        }

        public void Seed(IRuntimeContext? _, int seed) => _random = new Random(seed);
        public double Rand(IRuntimeContext? _) => _random.NextDouble();
        public int RandInt(IRuntimeContext? _) => _random.Next();

        public class Comparer : IComparer<BaseAgent>
        {
            public int Compare(BaseAgent? x, BaseAgent? y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return 1;
                if (y is null) return -1;
                if (x._comparisonId < y._comparisonId) return -1;
                if (x._comparisonId > y._comparisonId) return 1;
                return 0;
            }

        }
    }
}

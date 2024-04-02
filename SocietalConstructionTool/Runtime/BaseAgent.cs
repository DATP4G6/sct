namespace Sct.Runtime
{
    public abstract class BaseAgent
    {
        public string State { get; set; }
        public IDictionary<string, dynamic> Fields { get; set; }

        // This makes it so that the class name shows up in serialization
        public string ClassName { get; }

        public BaseAgent(string state, IDictionary<string, dynamic> fields)
        {
            State = state;
            Fields = fields;
            ClassName = GetType().Name;
        }

        public abstract void Update(IRuntimeContext ctx);
        public abstract BaseAgent Clone();
        public static string EnterMethodName => nameof(Enter);
        protected void Enter(IRuntimeContext ctx, string state)
        {
            State = state;
            ctx.AgentHandler.CreateAgent(this);
        }
    }
}

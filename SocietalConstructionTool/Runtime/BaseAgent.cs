namespace Sct.Runtime;
public abstract class BaseAgent(string state, IDictionary<string, dynamic> fields)
{
    public string State { get; set; } = state;
    public IDictionary<string, dynamic> Fields { get; set; } = fields.ToDictionary(x => x.Key, x => x.Value);
    public abstract void Update(IRuntimeContext ctx);
    public abstract BaseAgent Clone();
    // Public to allow nameof during compilation.
    // Should only ever be called in a protected manner.
    public static string EnterMethodName => nameof(Enter);
    protected void Enter(IRuntimeContext ctx)
    {
        ctx.AgentHandler.CreateAgent(this);
    }
}

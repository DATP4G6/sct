namespace Sct.Runtime;
public abstract class BaseAgent(string state, IDictionary<string, dynamic> fields)
{
    public string State { get; set; } = state;
    public IDictionary<string, dynamic> Fields { get; set; } = fields.ToDictionary(x => x.Key, x => x.Value);
    public abstract void Update(IRuntimeContext ctx);
    public abstract BaseAgent Clone();
}

namespace Sct.Runtime;
public abstract class BaseAgent
{
    public string State { get; set; }
    public IDictionary<string, dynamic> Fields { get; set; }
    public BaseAgent(string state, IDictionary<string, dynamic> fields)
    {
        State = state;
        Fields = fields;
    }
}

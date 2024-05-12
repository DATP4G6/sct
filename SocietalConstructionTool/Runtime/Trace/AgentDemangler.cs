namespace Sct.Runtime.Trace;

public class AgentDemangler(BaseAgent agents)
{
    public string State { get; } = agents.State.Remove(0, 6);
    public IDictionary<string, dynamic> Fields { get; } = agents.Fields.ToDictionary(k => k.Key.Remove(0, 6), v => v.Value);
    public string SpeciesName { get; } = agents.SpeciesName.Remove(0, 6);
}

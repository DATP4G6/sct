using System.Text.Json;

namespace Sct.Runtime.Trace
{
    public class JsonStringLogger : IOutputLogger
    {
        private readonly List<IEnumerable<BaseAgent>> _ticks = [];
        private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true, };
        public string? Output { get; private set; } = null;

        public void OnExit() => Output = JsonSerializer.Serialize(_ticks, SerializerOptions);

        public void OnTick(IRuntimeContext context) => _ticks.Add(context.AgentHandler.Agents.Select(a => (BaseAgent)a.Clone()));
    }
}

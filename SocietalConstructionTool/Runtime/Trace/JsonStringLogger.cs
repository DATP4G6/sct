using System.Text;
using System.Text.Json;

namespace Sct.Runtime.Trace
{
    public class JsonStringLogger : IOutputLogger
    {
        private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true, };
        private readonly StringBuilder _builder = new();
        public string? Output { get; private set; } = null;

        public void OnExit() => Output = _builder.ToString();

        public void OnTick(IRuntimeContext context) => _builder.Append(JsonSerializer.Serialize(context.AgentHandler.Agents, SerializerOptions));
    }
}

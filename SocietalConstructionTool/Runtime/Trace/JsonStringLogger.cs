using System.Text;
using System.Text.Json;

using Sct.Extensions;

namespace Sct.Runtime.Trace
{
    public class JsonStringLogger : IOutputLogger
    {
        private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true, };
        private readonly StringBuilder _builder = new();
        public string? Output { get; private set; } = null;

        public void OnExit() => Output = _builder.ToString();

        public void OnTick(IRuntimeContext context)
        {
            var ordered = context.AgentHandler.Agents.DeterministicOrder();
            _ = _builder.Append(JsonSerializer.Serialize(ordered, SerializerOptions) + '\n');
        }
    }
}

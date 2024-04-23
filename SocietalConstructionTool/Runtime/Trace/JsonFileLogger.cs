using System.Text;
using System.Text.Json;

using Sct.Extensions;

namespace Sct.Runtime.Trace
{
    public class JsonFileLogger(string destination) : IOutputLogger
    {
        private readonly StringBuilder _builder = new();

        public void OnExit()
        {
            using FileStream file = File.OpenWrite(destination);
            file.Write(Encoding.UTF8.GetBytes(_builder.ToString()));
        }

        public void OnTick(IRuntimeContext context)
        {
            var ordered = context.AgentHandler.Agents.DeterministicOrder();
            _ = _builder.Append(JsonSerializer.Serialize(ordered) + '\n');
        }
    }
}

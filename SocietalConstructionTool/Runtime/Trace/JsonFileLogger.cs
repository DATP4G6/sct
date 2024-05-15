using System.Text;
using System.Text.Json;

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
            => _builder.Append(
                JsonSerializer.Serialize(
                    context.AgentHandler.Agents.Select(a => new DemangledAgent(a))
                ) + '\n');
    }
}

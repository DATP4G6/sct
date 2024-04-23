using System.Text.Json;

using Sct.Extensions;

namespace Sct.Runtime.Trace
{
    public class JsonConsoleLogger : IOutputLogger
    {
        public void OnTick(IRuntimeContext context)
        {
            var ordered = context.AgentHandler.Agents.DeterministicOrder();
            Console.WriteLine(JsonSerializer.Serialize(ordered) + '\n');
        }
    }
}

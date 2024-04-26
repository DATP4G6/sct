using System.Text.Json;

namespace Sct.Runtime.Trace
{
    public class JsonConsoleLogger : IOutputLogger
    {
        public void OnTick(IRuntimeContext context)
             => Console.WriteLine(
                JsonSerializer.Serialize(
                    context.AgentHandler.Agents.Select(a => new AgentDemangler(a))));
    }
}

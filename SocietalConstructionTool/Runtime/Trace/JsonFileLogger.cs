using System.Text.Json;

namespace Sct.Runtime.Trace
{
    public class JsonFileLogger(string destination) : IOutputLogger
    {
        private readonly List<IEnumerable<BaseAgent>> _ticks = [];

        public void OnExit()
        {
            using FileStream file = File.OpenWrite(destination);
            JsonSerializer.Serialize(file, _ticks);
        }

        public void OnTick(IRuntimeContext context) => _ticks.Add(context.AgentHandler.Agents);
    }
}

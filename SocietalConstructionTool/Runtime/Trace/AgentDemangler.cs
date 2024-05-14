using Sct.Compiler.Translator;

namespace Sct.Runtime.Trace
{
    public class AgentDemangler(BaseAgent agents)
    {
        private static readonly int PrefixLength = TranslatorUtils.MangleNamePrefix.Length;

        public string Species { get; } = agents.ClassName.Remove(0, PrefixLength);
        public string State { get; } = agents.State.Remove(0, PrefixLength);
        public IDictionary<string, dynamic> Fields { get; } = agents.Fields.ToDictionary(k => k.Key.Remove(0, PrefixLength), v => v.Value);

        private string FieldsString => string.Join(", ", Fields.Select(kv => $"{kv.Key}: {kv.Value}"));
        public override string ToString() => $"{Species}::{State}({FieldsString})";
    }
}

using System.Collections.Immutable;

namespace Sct.Compiler.Typechecker
{
    public class CTable(Dictionary<string, ClassContent> classes, ClassContent globalClass)
    {
        private readonly Dictionary<string, ClassContent> _classes = classes;
        public ImmutableDictionary<string, ClassContent> Classes => _classes.ToImmutableDictionary();
        public ClassContent GlobalClass { get; } = globalClass;

        public ClassContent? GetClassContent(string className) => _classes.TryGetValue(className, out var classContent) ? classContent : null;
    }
}

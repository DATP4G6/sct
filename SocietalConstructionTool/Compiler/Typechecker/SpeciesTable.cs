using System.Collections.Immutable;

namespace Sct.Compiler.Typechecker
{
    public class CTable(Dictionary<string, SpeciesContent> species, SpeciesContent globalContent)
    {
        private readonly Dictionary<string, SpeciesContent> _species = species;
        public ImmutableDictionary<string, SpeciesContent> Species => _species.ToImmutableDictionary();
        public SpeciesContent GlobalContent { get; } = globalContent;

        public SpeciesContent? GetSpeciesContent(string speciesName) => _species.TryGetValue(speciesName, out var speciesContent) ? speciesContent : null;
    }
}

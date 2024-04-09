using Antlr4.Runtime;

using Microsoft.CodeAnalysis;

using Sct.Compiler.Translator;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class TranslatorTests : AbstractSnapshotTests
    {
        private static new IEnumerable<string[]> Files => AbstractSnapshotTests.Files;

        // Run each file as a seperate test
        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task TranslateFile(string file)
        {
            UseProjectRelativeDirectory("Snapshots/TranslatorTests"); // save snapshots here

            string input = File.ReadAllText(file);
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new SctLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            SctParser parser = new(tokens);
            var listener = new SctTranslator();
            parser.AddParseListener(listener);
            _ = parser.start();

            _ = await Verify(listener.Root?.NormalizeWhitespace().ToFullString())
                .UseFileName(Path.GetFileNameWithoutExtension(file));
        }
    }
}

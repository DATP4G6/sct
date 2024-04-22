using Microsoft.CodeAnalysis;

using Sct;
using Sct.Compiler.Translator;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class TranslatorTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> Files => GetTestFiles("Parser");

        // Run each file as a seperate test
        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task TranslateFile(string file)
        {
            UseProjectRelativeDirectory("Snapshots/TranslatorTests"); // save snapshots here

            SctParser parser = await SctRunner.GetParserAsync(file);
            var listener = new SctTranslator();
            parser.AddParseListener(listener);
            _ = parser.start();

            _ = await Verify(listener.Root?.NormalizeWhitespace().ToFullString())
                .UseFileName(Path.GetFileNameWithoutExtension(file));
        }
    }
}

using Microsoft.CodeAnalysis;

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
            var ast = TestFileUtils.BuildAst(file);
            var visitor = new SctAstTranslator();
            var tree = ast.Accept(visitor);

            _ = await Verify(tree.NormalizeWhitespace().ToFullString())
                .UseFileName(Path.GetFileNameWithoutExtension(file));
        }
    }
}

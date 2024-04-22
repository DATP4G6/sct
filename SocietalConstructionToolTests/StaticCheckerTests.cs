using Sct;
using Sct.Compiler.Typechecker;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class StaticCheckerTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> Files => GetTestFiles("StaticCheckTests");

        // Run each file as a seperate test
        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task RunFile(string file)
        {
            UseProjectRelativeDirectory("Snapshots/StaticCheckTests"); // save snapshots here

            SctParser parser = await SctRunner.GetParserAsync(file);
            var startNode = parser.start();

            KeywordContextCheckVisitor keywordChecker = new();
            var errors = startNode.Accept(keywordChecker);

            _ = await Verify(errors)
                .UseFileName(Path.GetFileNameWithoutExtension(file));
        }
    }
}

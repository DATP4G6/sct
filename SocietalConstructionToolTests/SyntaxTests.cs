using Sct;

using Sct.Compiler;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class SyntaxTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> Files => GetTestFiles("SyntaxTests");

        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task ParseFile(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/SyntaxTests"); // save snapshots here
            var snapshotFileName = Path.GetFileNameWithoutExtension(testFile);

            var parser = await SctRunner.GetParserAsync(testFile);
            var errorListener = new SctErrorListener();
            parser.AddErrorListener(errorListener);
            _ = parser.start();

            _ = await Verify(errorListener.Errors)
                .UseFileName(snapshotFileName);
        }
    }
}

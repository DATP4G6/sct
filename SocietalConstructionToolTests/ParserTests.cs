using Sct;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class ParserTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> Files => GetTestFiles("Parser");

        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task ParseFile(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/ParserTests"); // save snapshots here

            var parser = await SctRunner.GetParserAsync(testFile);
            var result = parser.start();

            _ = await Verify(result.ToStringTree(parser))
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }
    }
}

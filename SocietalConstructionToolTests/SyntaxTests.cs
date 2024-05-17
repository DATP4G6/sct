using Sct;

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

            var (_, errors) = SctRunner.CompileSct([testFile]);

            _ = await Verify(errors)
                .UseFileName(snapshotFileName);
        }
    }
}

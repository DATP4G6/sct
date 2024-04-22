using Sct;
using Sct.Compiler;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class StaticCheckTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> Files => GetTestFiles("StaticCheckTests");

        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task TestStaticChecks(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/StaticCheckTests"); // save snapshots here
            List<CompilerError> errors = SctRunner.RunStaticChecks([testFile]);

            _ = await Verify(errors)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }
    }
}

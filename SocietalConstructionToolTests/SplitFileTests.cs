using Microsoft.CodeAnalysis;

using Sct;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class SplitFileTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> Files => GetTestFiles("SplitFileTests");

        [DataTestMethod]
        public async Task RunFiles()
        {
            UseProjectRelativeDirectory("Snapshots/SplitFileTests"); // save snapshots here

            var files = GetFiles();

            var (outputText, errors) = SctRunner.CompileSct(files);

            Assert.IsFalse(errors.Any(), string.Join("\n", errors));
            Assert.IsNotNull(outputText);
            _ = await Verify(outputText)
                .UseFileName(Path.GetFileNameWithoutExtension(files[0]));
        }

        // RunFiles is run for each file, and passing files to CompileSct only passes the file that triggered the test.
        // This method is used to get all files to pass to CompileSct.
        private static string[] GetFiles()
        {
            return Files.SelectMany(f => f).Order().ToArray();
        }
    }
}

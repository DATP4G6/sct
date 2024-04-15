using Microsoft.CodeAnalysis;

using Sct;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class SplitFileTests : AbstractSnapshotTests
    {
        private static new IEnumerable<string[]> Files =>
            Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "SplitFileTests"))
            .Select(f => new[] { f });


        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task RunFiles(params string[] files)
        {
            UseProjectRelativeDirectory("Snapshots/SplitFileTests"); // save snapshots here

            var (outputText, errors) = SctRunner.CompileSct(GetFiles());

            Assert.IsTrue(errors.Count() == 0, string.Join("\n", errors));
            _ = await Verify(outputText)
                .UseFileName(Path.GetFileNameWithoutExtension(files[0]));
        }

        // RunFiles is run for each file, and passing files to CompileSct only passes the file that triggered the test.
        // This method is used to get all files to pass to CompileSct.
        private static string[] GetFiles()
        {
            return Files.SelectMany(f => f).ToArray();
        }
    }
}

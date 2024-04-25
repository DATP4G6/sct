using Sct;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class CompilerTests
    {
        private static IEnumerable<string[]> Files => TestFileUtils.GetTestFiles("CompilerTests");

        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public void ParseFile(string testFile)
        {
            var (outputText, errors) = SctRunner.CompileSct([testFile]);

            Assert.IsFalse(errors.Any(), "SCT compiled with errors:\n" + string.Join('\n', errors));
            Assert.IsNotNull(outputText);

            // Discard the generated assembly
            var stream = Stream.Null;

            var result = SctRunner.Emit(outputText, stream);
            Assert.IsTrue(result.Success, $"Could not compile generated C#\nCompilation diagnostics: {string.Join('\n', result.Diagnostics)}\n\nCode:\n" + outputText);
        }
    }
}

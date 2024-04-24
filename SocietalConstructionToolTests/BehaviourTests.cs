using Sct;
using Sct.Runtime.Trace;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class BehaviourTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> Files => GetTestFiles("BehaviourTests");

        // Run each file as a seperate test
        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task RunFile(string file)
        {
            UseProjectRelativeDirectory("Snapshots/BehaviourTests"); // save snapshots here

            JsonStringLogger logger = new();

            var errors = SctRunner.CompileAndRun([file], logger);

            Assert.IsFalse(errors.Any(), "Tried to run code with errors" + string.Join('\n', errors));

            _ = await Verify(logger.Output)
                .UseFileName(Path.GetFileNameWithoutExtension(file));
        }
    }
}

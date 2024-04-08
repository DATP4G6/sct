using Microsoft.CodeAnalysis;

using Sct;
using Sct.Runtime.Trace;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class BehaviourTests : AbstractSnapshotTests
    {

        private static new IEnumerable<string[]> Files =>
            Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "BehaviourTests"))
            .Select(f => new[] { f });

        // Run each file as a seperate test
        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task RunFile(string file)
        {
            UseProjectRelativeDirectory("Snapshots/BehaviourTests"); // save snapshots here

            JsonStringLogger logger = new();

            SctRunner.CompileAndRun([file], logger);

            _ = await Verify(logger.Output)
                .UseFileName(Path.GetFileNameWithoutExtension(file));
        }
    }

}

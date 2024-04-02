using Sct.Runtime;
using Sct.Runtime.Trace;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class RuntimeContextFactoryTests
    {
        [TestMethod]
        public void TestParsePrintCommand()
        {
            string[] args = ["print"];
            var ctx = RuntimeContextFactory.CreateFromArgs(args);
            Assert.IsInstanceOfType(ctx!.OutputLogger, typeof(JsonConsoleLogger));
        }

        [TestMethod]
        public void TestParseWriteCommand()
        {
            string[] args = ["write", "-o", "test.txt"];
            var ctx = RuntimeContextFactory.CreateFromArgs(args);
            Assert.IsInstanceOfType(ctx!.OutputLogger, typeof(JsonFileLogger));
        }

        [TestMethod]
        public void TestParseWriteCommandFailsWithoutFile()
        {
            string[] args = ["write"];
            var ctx = RuntimeContextFactory.CreateFromArgs(args);
            Assert.IsNull(ctx);
        }
    }
}

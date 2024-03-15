using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class ParserTests : VerifyBase
    {
        private static string TestFilesDirectory => Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestFiles");

        [ClassInitialize]
        public static void Setup(TestContext _)
        {
            DiffEngine.DiffRunner.Disabled = true;
            UseProjectRelativeDirectory("Snapshots");
        }

        [TestMethod]
        public Task TestParseAST()
        {
            string[] testFiles = Directory.GetFiles(TestFilesDirectory);
            return Task.WhenAll(testFiles.AsParallel().Select(TestFile));
        }

        public async Task TestFile(string testFile)
        {
            string input = await File.ReadAllTextAsync(testFile);
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new SctLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            SctParser parser = new(tokens);
            IParseTree result = parser.start();
            _ = await Verify(result.ToStringTree(parser)).UseMethodName("TestParseAST." + Path.GetFileNameWithoutExtension(testFile));
        }

        private static string GetTestFile(string testName)
        {
            return Path.Join(TestFilesDirectory, testName);
        }
    }
}

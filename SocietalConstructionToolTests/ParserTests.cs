using Antlr4.Runtime;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class ParserTests : AbstractSnapshotTests
    {
        private static new IEnumerable<string[]> Files => AbstractSnapshotTests.Files;

        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task TestFile(string testFile)
        {
            string input = await File.ReadAllTextAsync(testFile);
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new SctLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            SctParser parser = new(tokens);
            SctParser.StartContext result = parser.start();
            _ = await Verify(result.ToStringTree(parser)).UseMethodName("TestParseAST." + Path.GetFileNameWithoutExtension(testFile));
        }
    }
}

using Antlr4.Runtime;

using Sct.Compiler;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class SyntaxTests : AbstractSnapshotTests
    {
        private static new IEnumerable<string[]> Files =>
           Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "SyntaxTests"))
           .Select(f => new[] { f });

        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task ParseFile(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/SyntaxTests"); // save snapshots here
            var snapshotFileName = Path.GetFileNameWithoutExtension(testFile);

            string input = await File.ReadAllTextAsync(testFile);
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new SctLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            SctParser parser = new(tokens);
            var errorListener = new SctErrorListener();
            parser.AddErrorListener(errorListener);
            _ = parser.start();

            _ = await Verify(errorListener.Errors)
                .UseFileName(snapshotFileName);
        }
    }
}

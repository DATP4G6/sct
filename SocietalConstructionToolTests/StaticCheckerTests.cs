using Antlr4.Runtime;

using Sct.Compiler.Typechecker;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class StaticCheckerTests : AbstractSnapshotTests
    {

        private static new IEnumerable<string[]> Files =>
            Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "StaticCheckTests"))
            .Select(f => new[] { f });

        // Run each file as a seperate test
        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task RunFile(string file)
        {
            UseProjectRelativeDirectory("Snapshots/StaticCheckTests"); // save snapshots here

            string input = File.ReadAllText(file);
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new SctLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            SctParser parser = new(tokens);

            var startNode = parser.start();

            KeywordContextCheckVisitor keywordChecker = new();
            var errors = startNode.Accept(keywordChecker);

            _ = await Verify(errors)
                .UseFileName(Path.GetFileNameWithoutExtension(file));
        }
    }

}

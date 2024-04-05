using Antlr4.Runtime;

using Sct.Compiler;
using Sct.Compiler.Typechecker;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class TypeCheckerTests : VerifyBase
    {
        private static IEnumerable<string[]> Files =>
            Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestFiles/Typechecker"))
            .Select(f => new[] { f });

        [ClassInitialize]
        public static void Setup(TestContext _)
        {
            DiffEngine.DiffRunner.Disabled = true; // avoid destroying your terminal
        }

        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task TypecheckFile(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/TypeCheckerTests"); // save snapshots here

            string input = await File.ReadAllTextAsync(testFile);
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new SctLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);

            List<CompilerError> errors = new();
            SctParser parser = new(tokens);
            SctParser.StartContext startNode = parser.start();

            var returnChecker = new SctReturnCheckVisitor();
            _ = startNode.Accept(returnChecker);
            errors.AddRange(returnChecker.Errors);

            var sctTableVisitor = new SctTableVisitor();
            _ = sctTableVisitor.Visit(startNode);
            var ctable = sctTableVisitor.Ctable;
            errors.AddRange(sctTableVisitor.Errors);

            var sctTypeChecker = new SctTypeChecker(ctable!);
            _ = startNode.Accept(sctTypeChecker);

            errors.AddRange(sctTypeChecker.Errors);

            var errorString = string.Join(Environment.NewLine, errors.Select(e => e.ToString()));

            _ = await Verify(errorString)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }
    }
}

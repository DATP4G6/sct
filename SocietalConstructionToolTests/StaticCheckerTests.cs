using Sct;
using Sct.Compiler;
using Sct.Compiler.Typechecker;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class StaticCheckerTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> Files => GetTestFiles("StaticCheckTests");

        // Run each file as a seperate test
        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task RunFile(string file)
        {
            UseProjectRelativeDirectory("Snapshots/StaticCheckTests"); // save snapshots here

            List<CompilerError> errors = new();
            SctParser parser = await SctRunner.GetParserAsync(file);
            var startNode = parser.start();

            KeywordContextCheckVisitor keywordChecker = new();
            var keywordErrors = startNode.Accept(keywordChecker).ToList();
            errors.AddRange(keywordErrors);

            var returnChecker = new SctReturnCheckVisitor();
            _ = startNode.Accept(returnChecker);
            errors.AddRange(returnChecker.Errors);

            _ = await Verify(errors)
                .UseFileName(Path.GetFileNameWithoutExtension(file));
        }
    }
}

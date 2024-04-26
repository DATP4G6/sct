using Sct;
using Sct.Compiler.Syntax;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class AstTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> Files => GetTestFiles("Parser");

        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task TestBuildAst(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast"); // Save snapshots here
            var parser = SctRunner.GetParser(testFile);
            var listener = new AstBuilderVisitor();
            var ast = (SctProgramSyntax)parser.start().Accept(listener);

            _ = await Verify(ast)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }
    }
}

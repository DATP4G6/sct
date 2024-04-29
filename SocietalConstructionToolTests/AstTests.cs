using Sct.Compiler.Typechecker;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class AstTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> ParserFiles => GetTestFiles("Parser");
        private static IEnumerable<string[]> StaticFiles => GetTestFiles("StaticCheckTests");

        /// <summary>
        /// Test that the return check visitor works correctly.
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(StaticFiles), DynamicDataSourceType.Property)]
        public async Task TestReturnCheckAst(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/Static"); // Save snapshots here
            var ast = TestFileUtils.BuildAst(testFile);
            var visitor = new SctReturnCheckAstVisitor();
            _ = ast.Accept(visitor);
            var errors = visitor.Errors;

            _ = await Verify(errors)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }
    }
}

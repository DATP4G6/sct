using Sct;
using Sct.Compiler.Syntax;
using Sct.Compiler.Typechecker;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class AstTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> ParserFiles => GetTestFiles("Parser");
        private static IEnumerable<string[]> StaticFiles => GetTestFiles("StaticCheckTests");

        private static SctProgramSyntax BuildAst(string testFile)
        {
            var parser = SctRunner.GetParser(testFile);
            var listener = new AstBuilderVisitor();
            return (SctProgramSyntax)parser.start().Accept(listener);
        }

        /// <summary>
        /// Test that the return check visitor works correctly.
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(StaticFiles), DynamicDataSourceType.Property)]
        public async Task TestReturnCheckAst(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/Static"); // Save snapshots here
            var ast = BuildAst(testFile);
            var visitor = new SctReturnCheckAstVisitor();
            _ = ast.Accept(visitor);
            var errors = visitor.Errors;

            _ = await Verify(errors)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }
    }
}

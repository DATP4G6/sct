using System.Text.Json;

using Sct;
using Sct.Compiler;
using Sct.Compiler.Syntax;
using Sct.Compiler.Typechecker;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class AstTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> ParserFiles => GetTestFiles("Parser");
        private static IEnumerable<string[]> StaticFiles => GetTestFiles("StaticCheckTests");
        private static IEnumerable<string[]> FoldingFiles => GetTestFiles("FoldingTests");

        private static VerifySettings IgnoreContext
        {
            get
            {
                var settings = new VerifySettings();
                settings.IgnoreMember<SctSyntax>(x => x.Context);
                settings.IgnoreMember<SctSyntax>(x => x.Children);
                return settings;
            }
        }

        private static SctProgramSyntax BuildAst(string testFile)
        {
            var parser = SctRunner.GetParser(testFile);
            var listener = new AstBuilderVisitor();
            return (SctProgramSyntax)parser.start().Accept(listener);
        }

        [DataTestMethod]
        [DynamicData(nameof(ParserFiles), DynamicDataSourceType.Property)]
        public async Task TestAst(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/Trees"); // Save snapshots here
            var ast = BuildAst(testFile);
            _ = await Verify(ast, IgnoreContext)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }

        /// <summary>
        /// Test that the return check visitor works correctly.
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(StaticFiles), DynamicDataSourceType.Property)]
        public async Task TestReturnCheckAst(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/ReturnCheck"); // Save snapshots here
            var ast = BuildAst(testFile);
            var visitor = new SctReturnCheckAstVisitor();
            _ = ast.Accept(visitor);
            var errors = visitor.Errors;

            _ = await Verify(errors)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }

        /// <summary>
        /// Test that the base builder correctly clones the AST.
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(ParserFiles), DynamicDataSourceType.Property)]
        public void TestCloneAst(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/Clone"); // Save snapshots here
            var ast = BuildAst(testFile);
            var visitor = new SctBaseBuilderSyntaxVisitor();
            var clonedAst = (SctProgramSyntax)ast.Accept(visitor);

            var astComparer = new SctAstComparer();

            // ASTs should be distinct
            Assert.IsTrue(astComparer.DeepReferenceDistinct(ast, clonedAst), "Cloned ast was not entirely distinct from original.");

            // ASTs content should be equal
            var astJson = JsonSerializer.Serialize(ast);
            var clonedAstJson = JsonSerializer.Serialize(clonedAst);
            Assert.IsTrue(astJson == clonedAstJson, "Cloned ast was not equal to original.");
        }

        /// <summary>
        /// Test that constants are folded correctly.
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(FoldingFiles), DynamicDataSourceType.Property)]
        public async Task TestFoldingAst(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/Folding"); // Save snapshots here
            var ast = BuildAst(testFile);
            var visitor = new AstFolderSyntaxVisitor();
            var foldedAst = (SctProgramSyntax)ast.Accept(visitor);

            _ = await Verify(foldedAst, IgnoreContext)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }

        /// <summary>
        /// Test that constants are folded correctly.
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(FoldingFiles), DynamicDataSourceType.Property)]
        public async Task TestFoldingErrorsAst(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/FoldingErrors"); // Save snapshots here
            var ast = BuildAst(testFile);
            var visitor = new AstFolderSyntaxVisitor();
            var foldedAst = (SctProgramSyntax)ast.Accept(visitor);
            foldedAst.ForceEvaluation();

            _ = await Verify(visitor.Errors.ToList())
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }
    }
}

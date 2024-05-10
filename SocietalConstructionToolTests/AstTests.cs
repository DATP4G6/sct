using System.Text.Json;

using Microsoft.CodeAnalysis;

using Sct;
using Sct.Compiler;
using Sct.Compiler.Syntax;
using Sct.Compiler.Translator;
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
                settings.IgnoreMember<SctClassSyntax>(x => x.Body);
                return settings;
            }
        }


        [DataTestMethod]
        [DynamicData(nameof(ParserFiles), DynamicDataSourceType.Property)]
        public async Task TestAst(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/Trees"); // Save snapshots here
            var ast = TestFileUtils.BuildAst(testFile);
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
            var ast = TestFileUtils.BuildAst(testFile);
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
            var ast = TestFileUtils.BuildAst(testFile);
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
        /// Test that the AST translator works correctly.
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(ParserFiles), DynamicDataSourceType.Property)]
        public async Task TestTranslatorAst(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/Translator"); // Save snapshots here
            var ast = TestFileUtils.BuildAst(testFile);
            var visitor = new SctAstTranslator();
            var tree = ast.Accept(visitor);

            _ = await Verify(tree.NormalizeWhitespace().ToFullString())
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }

        /// <summary>
        /// Test that the filename visitor works correctly
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(StaticFiles), DynamicDataSourceType.Property)]
        public async Task TestFilenameVisitor(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/Filename"); // Save snapshots here
            var ast = TestFileUtils.BuildAst(testFile);
            var filenameVisitor = new AstFilenameVisitor(testFile);
            var tree = ast.Accept(filenameVisitor);
            var errors = SctRunner.RunStaticChecks((SctProgramSyntax)tree);

            _ = await Verify(errors)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }


        /// <summary>
        /// Test that constants are folded correctly.
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(FoldingFiles), DynamicDataSourceType.Property)]
        public async Task TestFoldingAst(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/Folding"); // Save snapshots here
            var ast = TestFileUtils.BuildAst(testFile);
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
            var ast = TestFileUtils.BuildAst(testFile);
            var visitor = new AstFolderSyntaxVisitor();
            var foldedAst = (SctProgramSyntax)ast.Accept(visitor);
            foldedAst.ForceEvaluation();

            _ = await Verify(visitor.Errors.ToList())
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }

        /// <summary>
        /// Test that the table builder visitor works correctly.
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(StaticFiles), DynamicDataSourceType.Property)]
        public async Task TestTableBuilder(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/Table"); // Save snapshots here
            var ast = TestFileUtils.BuildAst(testFile);
            var cTableBuilder = new CTableBuilder();
            var visitor = new SctAstTableBuilderVisitor(cTableBuilder);
            _ = ast.Accept(visitor);

            var (table, errors) = cTableBuilder.BuildCtable();
            errors.AddRange(visitor.Errors);

            var settings = IgnoreContext;
            // Include CTable classes
            settings.AlwaysIncludeMembersWithType(typeof(Dictionary<string, ClassContent>));
            settings.AlwaysIncludeMembersWithType(typeof(Dictionary<string, FunctionType>));

            _ = await Verify(table, settings)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));

            UseProjectRelativeDirectory("Snapshots/Ast/Table/Errors");
            _ = await Verify(errors)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }

        /// <summary>
        /// Test that the type checker visitor works correctly.
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(StaticFiles), DynamicDataSourceType.Property)]
        public async Task TestTypeChecker(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/TypeCheck"); // Save snapshots here

            // Build the CTable
            var ast = TestFileUtils.BuildAst(testFile);

            var cTableBuilder = new CTableBuilder();
            var tableVisitor = new SctAstTableBuilderVisitor(cTableBuilder);
            _ = ast.Accept(tableVisitor);

            var (table, _) = cTableBuilder.BuildCtable();

            // Type check the AST
            var typeChecker = new SctAstTypeChecker(table);
            _ = typeChecker.Visit(ast);
            var typeErrors = typeChecker.Errors;

            _ = await Verify(typeErrors)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }

        /// <summary>
        /// Test that keyword usage is checked correctly.
        /// </summary>
        [DataTestMethod]
        [DynamicData(nameof(StaticFiles), DynamicDataSourceType.Property)]
        public async Task TestKeywordCheckAst(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/Ast/Keywords"); // Save snapshots here
            var ast = TestFileUtils.BuildAst(testFile);
            var visitor = new KeywordContextCheckSyntaxVisitor();
            var errors = ast.Accept(visitor);

            _ = await Verify(errors)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }
    }
}

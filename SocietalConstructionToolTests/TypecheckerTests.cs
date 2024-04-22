using Sct;
using Sct.Compiler;
using Sct.Compiler.Typechecker;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class TypeCheckerTests : AbstractSnapshotTests
    {
        private static IEnumerable<string[]> Files => GetTestFiles("Typechecker");

        [DataTestMethod]
        [DynamicData(nameof(Files), DynamicDataSourceType.Property)]
        public async Task TypecheckFile(string testFile)
        {
            UseProjectRelativeDirectory("Snapshots/TypeCheckerTests"); // save snapshots here

            SctParser parser = await SctRunner.GetParserAsync(testFile);
            SctParser.StartContext startNode = parser.start();
            List<CompilerError> errors = new();

            var returnChecker = new SctReturnCheckVisitor();
            _ = startNode.Accept(returnChecker);
            errors.AddRange(returnChecker.Errors);

            var cTableBuilder = new CTableBuilder();

            var sctTableVisitor = new SctTableVisitor(cTableBuilder);
            _ = sctTableVisitor.Visit(startNode);
            var ctable = cTableBuilder.BuildCtable();

            var setupType = ctable.GlobalClass.LookupFunctionType("Setup");
            if (setupType is null)
            {
                errors.Add(new CompilerError("No setup function found"));
            }
            else if (setupType.ReturnType != TypeTable.Void || setupType.ParameterTypes.Count != 0)
            {
                errors.Add(new CompilerError("Setup function must return void and take no arguments"));
            }

            errors.AddRange(sctTableVisitor.Errors);

            var sctTypeChecker = new SctTypeChecker(ctable);
            _ = startNode.Accept(sctTypeChecker);

            errors.AddRange(sctTypeChecker.Errors);

            var errorString = string.Join(Environment.NewLine, errors.Select(e => e.ToString()));

            _ = await Verify(errorString)
                .UseFileName(Path.GetFileNameWithoutExtension(testFile));
        }
    }
}

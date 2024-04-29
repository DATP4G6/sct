using Sct;
using Sct.Compiler.Syntax;

namespace SocietalConstructionToolTests
{
    public static class TestFileUtils
    {
        // yield all files in some subdirectory of the TestFiles directory
        public static IEnumerable<string[]> GetTestFiles(string dir) =>
            Directory.GetFiles(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", dir))
                .Select(f => new[] { f });

        public static SctProgramSyntax BuildAst(string testFile)
        {
            var parser = SctRunner.GetParser(testFile);
            var listener = new AstBuilderVisitor();
            return (SctProgramSyntax)parser.start().Accept(listener);
        }
    }
}

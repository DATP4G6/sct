using Antlr4.Runtime;

using Microsoft.CodeAnalysis;

using Sct.Compiler;

namespace SocietalConstructionToolTests
{
    [TestClass]
    public class TranslatorTests : VerifyBase
    {
        private static string TestFilesDirectory => Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestFiles");

        [ClassInitialize]
        public static void Setup(TestContext _)
        {
            DiffEngine.DiffRunner.Disabled = true;
            UseProjectRelativeDirectory("Snapshots");
        }

        [TestMethod]
        public Task TestCodeGeneration()
        {
            string[] testFiles = Directory.GetFiles(TestFilesDirectory);
            return Task.WhenAll(testFiles.AsParallel().Select(TranslateFile));
        }

        private async Task TranslateFile(string file)
        {
            string input = File.ReadAllText(file);
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new SctLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            SctParser parser = new(tokens);
            var listener = new SctTranslator();
            parser.AddParseListener(listener);
            _ = parser.start();

            _ = await Verify(listener.Root?.NormalizeWhitespace().ToFullString())
                .UseMethodName("TestCodeGeneration." + Path.GetFileNameWithoutExtension(file));
        }
    }
}

using System.Reflection;

using Antlr4.Runtime;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using Sct.Compiler;
using Sct.Compiler.Typechecker;
using Sct.Compiler.Translator;
using Sct.Runtime;
using Sct.Runtime.Trace;

namespace Sct
{
    public static class SctRunner
    {
        /**
         * <summary>
         * Reads an SCT source file, statically chekcs it and translates it into C# code
         * </summary>
         * <param name="filename">The path of the SCT source file</param>
         * <returns>The resulting C# source, or null if compilation failed</returns>
         */
        public static string? CompileSct(string filename)
        {
            // TODO: Add error handling
            string input = File.ReadAllText(filename);
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new SctLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            SctParser parser = new(tokens);

            var translator = new SctTranslator();
            parser.AddParseListener(translator);

            var startNode = parser.start();

            KeywordContextCheckVisitor keywordChecker = new();
            var errors = startNode.Accept(keywordChecker);

            if (errors.Length > 0)
            {
                string errorsText = string.Join('\n', (IEnumerable<CompilerError>)errors);
                // TODO: Better error handling
                throw new ArgumentException($"Could not compile SCT file.\nCompilation errors:\n{errorsText}");
            }

            // TODO: It's a little evil that we hold the entire generated source text in memory, but it is what it is
            return translator.Root?.NormalizeWhitespace().ToFullString();
        }

        /**
         * <summary>
         * Emits a piece of C# source text into a stream as an assembly
         * </summary>
         * <param name="sourceText">The C# source text</param>
         * <param name="outputStream">The stream to output into</param>
         * <returns>The result of emitting</returns>
         */
        public static EmitResult Emit(string sourceText, Stream outputStream)
        {
            string generatedAssemblyName = "sctGenerated";
            var tree = SyntaxFactory.ParseSyntaxTree(sourceText);

            Type[] referenceTypes = [typeof(object), typeof(IRuntimeContext), typeof(System.Dynamic.DynamicObject), typeof(Microsoft.CSharp.RuntimeBinder.Binder)];
            var references = Assembly.GetEntryAssembly()?.GetReferencedAssemblies().Select(r =>
                    MetadataReference.CreateFromFile(Assembly.Load(r).Location)
                    ).Concat(referenceTypes.Select(GetReferenceFromType))!;

            var compilation = CSharpCompilation.Create(generatedAssemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(tree);

            var path = Path.Combine(Directory.GetCurrentDirectory(), generatedAssemblyName);
            return compilation.Emit(outputStream);
        }

        private static PortableExecutableReference GetReferenceFromType(Type t)
        {
            var refLocation = t.GetTypeInfo().Assembly.Location;
            return MetadataReference.CreateFromFile(refLocation);
        }

        /**
         * <summary>
         * Run a generated SCT assembly
         * </summary>
         * <param name="assembly">The assembly to run</param>
         * <param name="logger">The logger to use to output the result of the simulation</param>
         */
        public static void Run(Assembly assembly, IOutputLogger logger)
        {
            IRuntimeContext ctx = RuntimeContextFactory.Create(logger);
            Run(assembly, ctx);
        }

        /**
         * <summary>
         * Run a generated SCT assembly
         * </summary>
         * <param name="assembly">The assembly to run</param>
         * <param name="initialContext">The initial context of the simulation</param>
         */
        public static void Run(Assembly assembly, IRuntimeContext initialContext)
        {
            var globalClassName = $"{SctTranslator.GeneratedNamespace}.{SctTranslator.GeneratedGlobalClass}";
            _ = assembly.GetType(globalClassName)?.GetMethod(SctTranslator.RunSimulationFunctionName)?.Invoke(null, [initialContext]);
        }

        /**
         * <summary>
         * Compile and run an SCT file
         * </summary>
         * <param name="filename">The path of the SCT source file</param>
         * <param name="logger">The logger to use to output the result of the simulation</param>
         */
        public static void CompileAndRun(string filename, IOutputLogger logger)
        {
            var outputSource = CompileSct(filename);
            if (outputSource is null)
            {
                // TODO: Error reporting
                Console.Error.WriteLine("Compiling SCT failed");
                return;
            }

            // Write the compiled C# into memory
            MemoryStream memoryStream = new();
            var result = Emit(outputSource, memoryStream);

            if (!result.Success)
            {
                // TODO: Make this not depend on printing to the console
                // This should throw an exception instead
                Console.Error.WriteLine("Compiling generated C# failed");
                Console.Error.WriteLine("Compilation diagnostics:");
                Console.Error.WriteLine(string.Join('\n', result.Diagnostics));
                return;
            }

            _ = memoryStream.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(memoryStream.ToArray());

            Run(assembly, logger);
        }
    }
}

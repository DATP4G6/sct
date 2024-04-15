using System.Reflection;

using Antlr4.Runtime;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using Sct.Compiler;
using Sct.Compiler.Translator;
using Sct.Compiler.Typechecker;
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
        public static (string? outputText, IEnumerable<CompilerError> errors) CompileSct(string filename)
        {
            // TODO: Add error handling
            string input = File.ReadAllText(filename);
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new SctLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            SctParser parser = new(tokens);
            var startNode = parser.start();

            KeywordContextCheckVisitor keywordChecker = new();
            var errors = startNode.Accept(keywordChecker).ToList();


            // Run visitor that populates the tables.
            var sctTableVisitor = new SctTableVisitor();
            _ = startNode.Accept(sctTableVisitor);
            var ctable = sctTableVisitor.CTable;
            errors.AddRange(sctTableVisitor.Errors);

            // Run visitor that checks the types.
            var sctTypeChecker = new SctTypeChecker(ctable);
            _ = startNode.Accept(sctTypeChecker);
            parser.Reset();

            errors.AddRange(sctTypeChecker.Errors);

            var translator = new SctTranslator();
            parser.AddParseListener(translator);
            _ = parser.start();

            if (errors.Count > 0)
            {
                return (null, errors);
            }

            if (translator.Root is null)
            {
                throw new InvalidOperationException("Translation failed");
            }


            // HACK: We do something bad in our translator somewhere that means that we produce a syntax tree that is not valid,
            // but its string representation is.
            // Take for example the expression `foo.bar`.
            // The correct way to represent this would (roughly) be: `(member_access (id "foo") (id "bar"))`
            // However, writing it as `(id "foo.bar")` produces the same string, but causes problems when C#
            // tries to traverse the tree. As far as I can tell, the error we get isn't very telling, so for now we just deal with it.
            var outputText = translator.Root.NormalizeWhitespace().ToFullString();

            return (outputText, []);
        }

        /**
         * <summary>
         * Emits a piece of C# source text into a stream as an assembly
         * </summary>
         * <param name="sourceText">The C# source text</param>
         * <param name="outputStream">The stream to output into</param>
         * <returns>The result of emitting</returns>
         */
        private static EmitResult Emit(string sourceText, Stream outputStream)
        {
            string generatedAssemblyName = "sctGenerated";
            var tree = SyntaxFactory.ParseSyntaxTree(sourceText);

            Type[] referenceTypes = [typeof(object), typeof(IRuntimeContext), typeof(System.Dynamic.DynamicObject), typeof(Microsoft.CSharp.RuntimeBinder.Binder)];
            // Dark magic, maybe we can just do `referenceTypes.Select(GetReferenceFromType)` instead
            var references = Assembly.GetEntryAssembly()?.GetReferencedAssemblies().Select(r =>
                    MetadataReference.CreateFromFile(Assembly.Load(r).Location)
                    ).Concat(referenceTypes.Select(GetReferenceFromType))!;

            var compilation = CSharpCompilation.Create(generatedAssemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(tree);

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
        private static void Run(Assembly assembly, IOutputLogger logger)
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
        private static void Run(Assembly assembly, IRuntimeContext initialContext)
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
        public static void CompileAndRun(string[] filenames, IOutputLogger logger)
        {
            // TODO: Actually concatenate the files. Isak is working on this.
            var filename = filenames[0];
            var (outputText, errors) = CompileSct(filename);

            if (errors.Any() || outputText is null)
            {
                Console.Error.WriteLine("Compilation failed:");
                Console.Error.WriteLine(string.Join('\n', errors));
                return;
            }

            // Write the compiled C# into memory
            MemoryStream memoryStream = new();
            var result = Emit(outputText, memoryStream);

            if (!result.Success)
            {
                throw new ArgumentException($"Could not compile generated C#\nCompilation diagnostics: {string.Join('\n', result.Diagnostics)}");
            }

            _ = memoryStream.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(memoryStream.ToArray());

            Run(assembly, logger);
        }
    }
}

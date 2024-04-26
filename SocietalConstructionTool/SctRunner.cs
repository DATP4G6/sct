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
        /// <summary>
        /// Return the parser for the given SCT source file
        /// </summary>
        /// <param name="filename">File to parse</param>
        /// <returns>The parser from ANTLR</returns>
        public static SctParser GetParser(string filename) =>
            GetSctParser(File.ReadAllText(filename));

        /// <summary>
        /// Return the parser for the given SCT source file asynchronously
        /// </summary>
        /// <param name="filename">File to parse</param>
        /// <returns>The parser from ANTLR</returns>
        public static async Task<SctParser> GetParserAsync(string filename) =>
            GetSctParser(await File.ReadAllTextAsync(filename));

        private static SctParser GetSctParser(string input)
        {
            ICharStream stream = CharStreams.fromString(input);
            ITokenSource lexer = new SctLexer(stream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            SctParser parser = new(tokens);
            parser.RemoveErrorListeners();
            return parser;
        }

        /**
         * <summary>
         * Reads an SCT source file, statically chekcs it and translates it into C# code
         * </summary>
         * <param name="filenames">The path of the SCT source file</param>
         * <returns>The resulting C# source, or null if compilation failed</returns>
         */
        public static (string? outputText, IEnumerable<CompilerError> errors) CompileSct(string[] filenames)
        {
            // Make SctTableVisitor take a CTableBuilder as a parameter
            // Analyse each file separately
            // Add file name to each found error.
            // Call CTabelBuilder.BuildCtable() after all files have been visited
            // Run the translator on all files concatenated.

            var errors = RunStaticChecks(filenames);

            if (errors.Count > 0)
            {
                return (null, errors);
            }

            // Concatenate all files into one string and run the translator on it.
            string fullInput = ConcatenateFiles(filenames);
            SctParser parser = GetSctParser(fullInput);

            var translator = new SctTranslator();
            parser.AddParseListener(translator);
            _ = parser.start();

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

        private static List<CompilerError> RunFirstPassChecks(ParserRuleContext startNode, CTableBuilder cTableBuilder)
        {
            var errors = new List<CompilerError>();

            KeywordContextCheckVisitor keywordChecker = new();
            var keywordErrors = startNode.Accept(keywordChecker).ToList();
            errors.AddRange(keywordErrors);

            SctReturnCheckVisitor returnChecker = new();
            _ = startNode.Accept(returnChecker);
            errors.AddRange(returnChecker.Errors);

            // Run visitor that populates the tables using the CTableBuilder.
            var sctTableVisitor = new SctTableVisitor(cTableBuilder);
            _ = startNode.Accept(sctTableVisitor);
            errors.AddRange(sctTableVisitor.Errors);
            return errors;
        }

        private static List<CompilerError> RunSecondPassChecks(ParserRuleContext startNode, CTable cTable)
        {
            var typeChecker = new SctTypeChecker(cTable);
            _ = startNode.Accept(typeChecker);
            return typeChecker.Errors.ToList();
        }

        public static List<CompilerError> RunStaticChecks(string[] filenames)
        {
            // Create a CTableBuilder that is used for all files.
            CTableBuilder cTableBuilder = new();
            var errors = new List<CompilerError>();

            // Store parses for each file to avoid having to recreate them for type checking.
            Dictionary<string, SctParser.StartContext> startNodes = [];

            // Run static analysis on each file separately.
            foreach (var file in filenames)
            {

                var parser = GetParser(file);

                //adds an error listener before the parser starts
                var errorListener = new SctErrorListener();
                parser.AddErrorListener(errorListener);

                // Save parser for later use.
                startNodes[file] = parser.start();
                var startNode = startNodes[file];

                // Run checks
                var fileErrors = RunFirstPassChecks(startNode, cTableBuilder);

                //adds syntax errors
                fileErrors.AddRange(errorListener.Errors.ToList());

                // Annotate each error with the filename.
                foreach (var error in fileErrors)
                {
                    error.Filename = file;
                }
                errors.AddRange(fileErrors);
            }

            // Build the CTable after all files have been visited.
            // The CTable is used for type checking.
            var (cTable, tableErrors) = cTableBuilder.BuildCtable();
            errors.AddRange(tableErrors);

            // Typecheck each file separately.
            // Identifiers from other files are known because the CTable is built from all files.
            foreach (var file in filenames)
            {
                var fileErrors = RunSecondPassChecks(startNodes[file], cTable);

                foreach (var error in fileErrors)
                {
                    error.Filename = file;
                }

                errors.AddRange(fileErrors);
            }

            return errors;
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
            // Dark magic, maybe we can just do `referenceTypes.Select(GetReferenceFromType)` instead
            var references = Assembly.GetEntryAssembly()?.GetReferencedAssemblies().Select(r =>
                    MetadataReference.CreateFromFile(Assembly.Load(r).Location)
                    ).Concat(referenceTypes.Select(GetReferenceFromType))!;

            var compilation = CSharpCompilation.Create(generatedAssemblyName)
                .WithOptions(
                        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                            .WithOptimizationLevel(OptimizationLevel.Release)
                        )
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
        private static void Run(Assembly assembly, IOutputLogger? logger)
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
        public static IEnumerable<CompilerError> CompileAndRun(string[] filenames, IOutputLogger? logger)
        {

            var (outputText, errors) = CompileSct(filenames);

            // TODO: Handle errors from ANTLR. They are not currently being passed to the errors list.
            if (errors.Any() || outputText is null)
            {
                Console.Error.WriteLine("Compilation failed:");
                Console.Error.WriteLine(string.Join('\n', errors));
                return errors;
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

            return [];
        }

        private static string ConcatenateFiles(string[] filenames)
        {

            string result = string.Empty;
            foreach (var file in filenames)
            {
                result += File.ReadAllText(file);
            }

            return result;
        }
    }
}

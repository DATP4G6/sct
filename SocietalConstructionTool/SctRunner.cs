using System.Reflection;

using Antlr4.Runtime;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using Sct.Compiler;
using Sct.Compiler.StaticChecks;
using Sct.Compiler.Syntax;
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

        private static (SctProgramSyntax astRoot, IEnumerable<CompilerError> errors) GetAst(SctParser parser)
        {
            var errorListener = new SctErrorListener();
            parser.AddErrorListener(errorListener);
            var visitor = new AstBuilderVisitor();
            // Calling `parser.start()` causes the text to be parsed, which emits any syntax errors to the error listener
            var astRoot = parser.start().Accept(visitor);
            return ((SctProgramSyntax)astRoot, errorListener.Errors);
        }

        // this allows us to include the stdlib compile-time
        private static string StdLib()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using Stream stream = asm.GetManifestResourceStream("SocietalConstructionTool.Resources.Stdlib.sct")!;
            using StreamReader source = new(stream);
            return source.ReadToEnd();
        }

        private static List<SctProgramSyntax> AddFilenameToContext(IEnumerable<SctProgramSyntax> astList, IEnumerable<string> filenames)
        {
            List<SctProgramSyntax> newAstList = [];
            foreach (var (ast, filename) in astList.Zip(filenames))
            {
                var filenameVisitor = new AstFilenameVisitor(filename);
                var newAst = ast.Accept(filenameVisitor);
                newAstList.Add((SctProgramSyntax)newAst);
            }
            return newAstList;
        }

        private static SctProgramSyntax GetMergedAst(IEnumerable<SctProgramSyntax> astList)
        {
            IEnumerable<SctClassSyntax> classes = astList.SelectMany(ast => ast.Classes);
            IEnumerable<SctFunctionSyntax> functions = astList.SelectMany(ast => ast.Functions);
            return new SctProgramSyntax(astList.First().Context, functions, classes);
        }

        /**
         * <summary>
         * Reads an SCT source file, statically checks it and translates it into C# code
         * </summary>
         * <param name="filenames">The path of the SCT source file</param>
         * <returns>The resulting C# source, or null if compilation failed</returns>
         */
        public static (string? outputText, IEnumerable<CompilerError> errors) CompileSct(IEnumerable<string> filenames)
        {
            // Add stdlib to the list of files to compile
            var parsers = filenames.Select(GetParser).Append(GetSctParser(StdLib()));
            var parserResults = parsers.Select(GetAst).ToList();

            var astList = parserResults.Select(pair => pair.astRoot);
            var parserErrors = parserResults.SelectMany(pair => pair.errors);

            // Don't continue if the syntax has errors, as the parse tree then might have nodes that are null, which can cause crashes later in the compiler
            if (parserErrors.Any())
            {
                return (null, parserErrors);
            }

            astList = AddFilenameToContext(astList, filenames.Append("stdlib"));

            var ast = GetMergedAst(astList);

            var errors = RunStaticChecks(ast);

            if (errors.Count > 0)
            {
                return (null, errors);
            }

            var translator = new SctTranslator();
            var tree = ast.Accept(translator);

            // HACK: We do something bad in our translator somewhere that means that we produce a syntax tree that is not valid,
            // but its string representation is.
            // Take for example the expression `foo.bar`.
            // The correct way to represent this would (roughly) be: `(member_access (id "foo") (id "bar"))`
            // However, writing it as `(id "foo.bar")` produces the same string, but causes problems when C#
            // tries to traverse the tree. As far as I can tell, the error we get isn't very telling, so for now we just deal with it.
            var outputText = tree.NormalizeWhitespace().ToFullString();

            return (outputText, []);
        }

        private static (List<CompilerError>, CTable) RunFirstPassChecks(SctProgramSyntax startNode)
        {
            var cTableBuilder = new CTableBuilder();
            var visitor = new SctTableBuilder(cTableBuilder);
            _ = startNode.Accept(visitor);
            var (table, errors) = cTableBuilder.BuildCtable();
            errors.AddRange(visitor.Errors);
            return (errors, table);
        }

        private static List<CompilerError> RunSecondPassChecks(SctProgramSyntax startNode, CTable table)
        {
            var typeChecker = new SctTypeChecker(table);
            _ = startNode.Accept(typeChecker);
            return typeChecker.Errors.ToList();
        }

        public static List<CompilerError> RunStaticChecks(SctProgramSyntax ast)
        {
            var errors = new List<CompilerError>();

            SctFolder folder = new();
            var foldedAst = (SctProgramSyntax)ast.Accept(folder);
            // tree is lazily evaluated, as it uses IEnumerable,
            // this forces the tree to evaluate to catch all errors
            foldedAst.ForceEvaluation();
            errors.AddRange(folder.Errors);

            SctKeywordContextChecker keywordChecker = new();
            var keywordErrors = foldedAst.Accept(keywordChecker).ToList();
            errors.AddRange(keywordErrors);

            var returnVisitor = new SctReturnChecker();
            _ = foldedAst.Accept(returnVisitor);
            errors.AddRange(returnVisitor.Errors);

            // The first pass of the typechecker that populates the tables using the CTableBuilder.
            var (firstPassErrors, table) = RunFirstPassChecks(foldedAst);
            errors.AddRange(firstPassErrors);

            // The second pass of the typechecker
            var secondPassErrors = RunSecondPassChecks(foldedAst, table);
            errors.AddRange(secondPassErrors);

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
        public static IEnumerable<CompilerError> CompileAndRun(IEnumerable<string> filenames, IOutputLogger? logger)
        {
            var fileErrors = filenames
                .Where(file => !File.Exists(file))
                .Select(file => new CompilerError($"Error: File '{file}' does not exist"));

            if (fileErrors.Any())
            {
                Console.Error.WriteLine(string.Join('\n', fileErrors));
                return fileErrors;
            }

            var (outputText, errors) = CompileSct(filenames);

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
    }
}

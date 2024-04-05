using Antlr4.Runtime;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Compiler;
using Sct.Compiler.Translator;
using Sct.Compiler.Typechecker;

static int SctParseMethod()
{
    List<CompilerError> errors = new();
    string input = File.ReadAllText("../SctBuildTasks/society.sct");
    ICharStream stream = CharStreams.fromString(input);
    ITokenSource lexer = new SctLexer(stream);
    ITokenStream tokens = new CommonTokenStream(lexer);
    SctParser parser = new SctParser(tokens);
    var startNode = parser.start();
    var returnChecker = new SctReturnCheckVisitor();
    _ = startNode.Accept(returnChecker);
    errors.AddRange(returnChecker.Errors);

    // Run visitor that populates the tables.
    var sctTableVisitor = new SctTableVisitor();
    _ = sctTableVisitor.Visit(startNode);
    var ctable = sctTableVisitor.Ctable;
    errors.AddRange(sctTableVisitor.Errors);

    // Run visitor that checks the types.
    var sctTypeChecker = new SctTypeChecker(ctable);
    _ = startNode.Accept(sctTypeChecker);
    parser.Reset();

    errors.AddRange(sctTypeChecker.Errors);
    if (errors.Count > 0)
    {
        foreach (var error in errors)
        {
            Console.WriteLine(error);
        }
        return 1;
    }

    // Run listener that translates the AST to C#.
    var listener = new SctTranslator();
    parser.AddParseListener(listener);
    _ = parser.start();

    if (listener.Root is not null)
        WriteNamespace(listener.Root);

    return 0;
}

static void WriteNamespace(NamespaceDeclarationSyntax ns)
{
    using var writer = new StreamWriter("MyClass.ignore.cs", false);
    ns.NormalizeWhitespace().WriteTo(writer);
}

return SctParseMethod();

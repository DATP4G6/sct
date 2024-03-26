

using Antlr4.Runtime;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Compiler;
using Sct.Compiler.Exceptions;

static void SctParseMethod()
{
    string input = File.ReadAllText("../SctBuildTasks/society.sct");
    ICharStream stream = CharStreams.fromString(input);
    ITokenSource lexer = new SctLexer(stream);
    ITokenStream tokens = new CommonTokenStream(lexer);
    SctParser parser = new SctParser(tokens);

    // Run visitor that populates the tables.
    var sctTableVisitor = new SctTableVisitor();
    _ = sctTableVisitor.Visit(parser.start());
    var ctable = sctTableVisitor.Ctable;
    parser.Reset();

    // Run visitor that checks the types.
    var SctTypeChecker = new SctTypeChecker(ctable);
    _ = parser.Accept(SctTypeChecker);
    parser.Reset();

    // Run listener that translates the AST to C#.
    var listener = new SctTranslator();
    parser.AddParseListener(listener);
    _ = parser.start();
    if (listener.Root is not null)
        WriteNamespace(listener.Root);
}

static void WriteNamespace(NamespaceDeclarationSyntax ns)
{
    using var writer = new StreamWriter("MyClass.ignore.cs", false);
    ns.NormalizeWhitespace().WriteTo(writer);
}

SctParseMethod();

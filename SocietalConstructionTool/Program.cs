

using Antlr4.Runtime;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Compiler;
using Sct.Compiler.Typechecking;

static void SctParseMethod()
{
    string input = File.ReadAllText("../SctBuildTasks/society.sct");
    ICharStream stream = CharStreams.fromString(input);
    ITokenSource lexer = new SctLexer(stream);
    ITokenStream tokens = new CommonTokenStream(lexer);
    SctParser parser = new SctParser(tokens);
    // var listener = new SctTranslator();
    // parser.AddParseListener(listener);
    var tableBuilder = new SctTableBuilder();
    parser.AddParseListener(tableBuilder);
    _ = parser.start();
    parser.Reset();
    parser.RemoveParseListeners();

    // Console.WriteLine("\n\");

    var typeChecker = new SctTypeChecker(tableBuilder.FTable, tableBuilder.STable, tableBuilder.DTable, tableBuilder.CTable);
    parser.AddParseListener(typeChecker);
    _ = parser.start();

    Console.WriteLine("\n\nErrors:");
    foreach (var error in tableBuilder.Errors.Concat(typeChecker.Errors))
    {
        Console.WriteLine(error);
    }

    // if (listener.Root is not null)
    //     WriteNamespace(listener.Root);
}

if (TypeTable.IsNumber(TypeTable.Int))
{
    Console.WriteLine("Int is a number");
}

static void WriteNamespace(NamespaceDeclarationSyntax ns)
{
    using var writer = new StreamWriter("MyClass.ignore.cs", false);
    ns.NormalizeWhitespace().WriteTo(writer);
}

SctParseMethod();

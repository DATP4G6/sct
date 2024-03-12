

using Antlr4.Runtime;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct;

void SctParseMethod()
{
    string input = File.ReadAllText("parser/society.sct");
    ICharStream stream = CharStreams.fromString(input);
    ITokenSource lexer = new SctLexer(stream);
    ITokenStream tokens = new CommonTokenStream(lexer);
    SctParser parser = new SctParser(tokens);
    var listener = new SctListener();
    parser.AddParseListener(listener);
    parser.start();
    WriteNamespace(listener.Namespace);
}

static void WriteNamespace(NamespaceDeclarationSyntax ns)
{
    using var writer = new StreamWriter("MyClass.cs", false);
    ns.NormalizeWhitespace().WriteTo(writer);
}

SctParseMethod();

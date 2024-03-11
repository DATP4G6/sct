

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

void SctParseMethod()
{
    string input = File.ReadAllText("parser/society.sct");
    ICharStream stream = CharStreams.fromString(input);
    ITokenSource lexer = new SctLexer(stream);
    ITokenStream tokens = new CommonTokenStream(lexer);
    SctParser parser = new SctParser(tokens);
    IParseTree tree = parser.start();
    Console.WriteLine(tree.ToStringTree(parser));
}

SctParseMethod();

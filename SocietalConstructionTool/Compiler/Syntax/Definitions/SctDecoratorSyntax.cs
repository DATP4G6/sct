namespace Sct.Compiler.Syntax
{
    public class SctDecoratorSyntax(string id, SctBlockStatementSyntax block) : SctDefinitionSyntax
    {
        public string Id => id;
        public SctBlockStatementSyntax Block => block;
    }
}

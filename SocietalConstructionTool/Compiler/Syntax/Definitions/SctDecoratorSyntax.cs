using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctDecoratorSyntax(ParserRuleContext context, string id, SctBlockStatementSyntax block) : SctDefinitionSyntax(context)
    {
        public string Id => id;
        public SctBlockStatementSyntax Block => block;
        public override IEnumerable<SctSyntax> Children => [Block];
    }
}

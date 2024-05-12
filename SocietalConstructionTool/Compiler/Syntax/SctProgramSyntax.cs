using Antlr4.Runtime;

namespace Sct.Compiler.Syntax
{
    public class SctProgramSyntax(ParserRuleContext context, IEnumerable<SctFunctionSyntax> functions, IEnumerable<SctSpeciesSyntax> species) : SctSyntax(context)
    {
        public IEnumerable<SctFunctionSyntax> Functions { get; } = functions;
        public IEnumerable<SctSpeciesSyntax> Species { get; } = species;
        public override IEnumerable<SctSyntax> Children => [.. Functions, .. Species];
    }
}

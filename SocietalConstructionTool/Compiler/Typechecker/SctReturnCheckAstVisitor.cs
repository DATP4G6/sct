using Sct.Compiler.Syntax;

namespace Sct.Compiler.Typechecker
{
    public class SctReturnCheckAstVisitor : SctBaseSyntaxVisitor<bool>, IErrorReporter
    {
        private readonly List<CompilerError> _errors = [];
        public IEnumerable<CompilerError> Errors => _errors;

        public override bool Visit(SctFunctionSyntax node)
        {
            var returns = node.ReturnType.Type switch
            {
                Syntax.SctType.Void => true,
                _ => node.Block.Accept(this)
            };

            if (!returns)
            {
                _errors.Add(new CompilerError($"Not all code paths return a value in function {node.Id}", node.Context));
            }
            return returns;
        }

        public override bool Visit(SctStateSyntax node)
        {
            var returns = node.Block.Accept(this);
            if (!returns)
            {
                _errors.Add(new CompilerError($"Not all code paths specify an end condition (`enter`, `destroy`, `exit`) in state {node.Id}", node.Context));
            }
            return returns;
        }

        public override bool Visit(SctBlockStatementSyntax node) => node.Statements.Any(s => s.Accept(this));
        public override bool Visit(SctIfStatementSyntax node) => node.Then.Accept(this) && (node.Else?.Accept(this) ?? true);
        public override bool Visit(SctElseStatementSyntax node) => node.Block.Accept(this);

        public override bool Visit(SctReturnStatementSyntax node) => true;
        public override bool Visit(SctEnterStatementSyntax node) => true;
        public override bool Visit(SctExitStatementSyntax node) => true;
        public override bool Visit(SctDestroyStatementSyntax node) => true;
    }
}

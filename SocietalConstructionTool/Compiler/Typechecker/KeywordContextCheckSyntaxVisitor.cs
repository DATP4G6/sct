using Sct.Compiler.Syntax;

namespace Sct.Compiler.Typechecker
{
    /// <summary>
    /// Checks that keywords are in the correct context,
    /// e.g. that break is only used within loops
    /// </summary>
    public class KeywordContextCheckSyntaxVisitor : SctBaseSyntaxVisitor<IEnumerable<CompilerError>>
    {

        private Context LocationContext { get; init; }

        protected override IEnumerable<CompilerError> DefaultResult => [];

        protected override IEnumerable<CompilerError> AggregateResult(IEnumerable<CompilerError> aggregate, IEnumerable<CompilerError> nextResult) => aggregate.Concat(nextResult);

        public override IEnumerable<CompilerError> Visit(SctFunctionSyntax node) => node.Block.Accept(WithFunctionContext());

        public override IEnumerable<CompilerError> Visit(SctStateSyntax node) => node.Block.Accept(WithStateContext());

        public override IEnumerable<CompilerError> Visit(SctDecoratorSyntax node) => node.Block.Accept(WithDecoratorContext());

        public override IEnumerable<CompilerError> Visit(SctWhileStatementSyntax node) => node.Block.Accept(WithLoopContext());

        public override IEnumerable<CompilerError> Visit(SctReturnStatementSyntax node) => LocationContext.CanReturn
            ? []
            : [ErrorFromContext(node.Context, "return")];

        public override IEnumerable<CompilerError> Visit(SctExitStatementSyntax node) => LocationContext.CanExit
            ? []
            : [ErrorFromContext(node.Context, "exit")];

        public override IEnumerable<CompilerError> Visit(SctEnterStatementSyntax node) => LocationContext.CanControlState
            ? []
            : [ErrorFromContext(node.Context, "enter")];

        public override IEnumerable<CompilerError> Visit(SctDestroyStatementSyntax node) => LocationContext.CanControlState
            ? []
            : [ErrorFromContext(node.Context, "destroy")];

        public override IEnumerable<CompilerError> Visit(SctBreakStatementSyntax node) => LocationContext.CanUseLoopControl
            ? []
            : [ErrorFromContext(node.Context, "break")];

        public override IEnumerable<CompilerError> Visit(SctContinueStatementSyntax node) => LocationContext.CanUseLoopControl
            ? []
            : [ErrorFromContext(node.Context, "continue")];

        public readonly struct Context
        {
            public bool CanControlState { get; init; }
            public bool CanExit { get; init; }
            public bool CanUseLoopControl { get; init; }
            public bool CanReturn { get; init; }
            public string ContextName { get; init; }
        }

        private static KeywordContextCheckSyntaxVisitor WithContext(Context ctx) => new()
        {
            LocationContext = ctx,
        };

        private KeywordContextCheckSyntaxVisitor WithStateContext() => WithContext(LocationContext with
        {
            CanControlState = true,
            CanExit = true,
            CanReturn = false,
            CanUseLoopControl = false,
            ContextName = "state",
        });

        private KeywordContextCheckSyntaxVisitor WithDecoratorContext() => WithContext(LocationContext with
        {
            CanControlState = true,
            CanExit = true,
            CanReturn = false,
            CanUseLoopControl = false,
            ContextName = "decorator",
        });

        private KeywordContextCheckSyntaxVisitor WithFunctionContext() => WithContext(LocationContext with
        {
            CanControlState = false,
            CanExit = false,
            CanReturn = true,
            CanUseLoopControl = false,
            ContextName = "function",
        });

        private KeywordContextCheckSyntaxVisitor WithLoopContext() => WithContext(LocationContext with
        {
            CanUseLoopControl = true,
        });

        private CompilerError ErrorFromContext(SctSyntaxContext context, string ruleName) =>
            new($"Cannot use '{ruleName}' inside {LocationContext.ContextName}", context);
    }
}

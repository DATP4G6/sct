using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Sct.Compiler.Typechecker
{
    /// <summary>
    /// Checks that keywords are in the correct context,
    /// e.g. that break is only used within loops
    /// </summary>
    public class KeywordContextCheckVisitor : SctBaseVisitor<CompilerError[]>
    {

        private Context LocationContext { get; init; }

        protected override CompilerError[] DefaultResult => [];

        protected override CompilerError[] AggregateResult(CompilerError[] aggregate, CompilerError[] nextResult) => aggregate.Concat(nextResult).ToArray();

        public override CompilerError[] VisitFunction([NotNull] SctParser.FunctionContext context) => context.statement_list().Accept(WithFunctionContext());

        public override CompilerError[] VisitState([NotNull] SctParser.StateContext context) => context.statement_list().Accept(WithStateContext());

        public override CompilerError[] VisitDecorator([NotNull] SctParser.DecoratorContext context) => context.statement_list().Accept(WithDecoratorContext());

        public override CompilerError[] VisitWhile([NotNull] SctParser.WhileContext context) => context.statement_list().Accept(WithLoopContext());

        public override CompilerError[] VisitReturn([NotNull] SctParser.ReturnContext context) => LocationContext.CanReturn
            ? []
            : [ErrorFromContext(context, "return")];

        public override CompilerError[] VisitExit([NotNull] SctParser.ExitContext context) => LocationContext.CanExit
            ? []
            : [ErrorFromContext(context, "exit")];

        public override CompilerError[] VisitEnter([NotNull] SctParser.EnterContext context) => LocationContext.CanControlState
            ? []
            : [ErrorFromContext(context, "enter")];

        public override CompilerError[] VisitDestroy([NotNull] SctParser.DestroyContext context) => LocationContext.CanControlState
            ? []
            : [ErrorFromContext(context, "destroy")];

        public override CompilerError[] VisitBreak([NotNull] SctParser.BreakContext context) => LocationContext.CanUseLoopControl
            ? []
            : [ErrorFromContext(context, "break")];

        public override CompilerError[] VisitContinue([NotNull] SctParser.ContinueContext context) => LocationContext.CanUseLoopControl
            ? []
            : [ErrorFromContext(context, "continue")];

        public readonly struct Context
        {
            public bool CanControlState { get; init; }
            public bool CanExit { get; init; }
            public bool CanUseLoopControl { get; init; }
            public bool CanReturn { get; init; }
            public string ContextName { get; init; }
        }

        private static KeywordContextCheckVisitor WithContext(Context ctx) => new()
        {
            LocationContext = ctx,
        };

        private KeywordContextCheckVisitor WithStateContext() => WithContext(LocationContext with
        {
            CanControlState = true,
            CanExit = true,
            CanReturn = false,
            CanUseLoopControl = false,
            ContextName = "state",
        });

        private KeywordContextCheckVisitor WithDecoratorContext() => WithContext(LocationContext with
        {
            CanControlState = true,
            CanExit = true,
            CanReturn = false,
            CanUseLoopControl = false,
            ContextName = "decorator",
        });

        private KeywordContextCheckVisitor WithFunctionContext() => WithContext(LocationContext with
        {
            CanControlState = false,
            CanExit = false,
            CanReturn = true,
            CanUseLoopControl = false,
            ContextName = "function",
        });

        private KeywordContextCheckVisitor WithLoopContext() => WithContext(LocationContext with
        {
            CanUseLoopControl = true,
        });

        private CompilerError ErrorFromContext(ParserRuleContext parserContext, string ruleName) =>
            new($"Cannot use '{ruleName}' inside {LocationContext.ContextName}", parserContext.Start.Line, parserContext.Start.Column);
    }
}

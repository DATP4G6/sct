namespace Sct.Compiler.Typechecker
{
    public class SctReturnCheckVisitor : SctBaseVisitor<bool>, IErrorReporter
    {
        private readonly List<CompilerError> _errors = new();
        public IEnumerable<CompilerError> Errors => _errors;

        public override bool VisitFunction(SctParser.FunctionContext context)
        {
            var @type = TypeTable.GetType(context.type().GetText());
            var returns = @type switch
            {
                { } when @type == TypeTable.Void => true,
                _ => context.statement_list().Accept(this)
            };

            if (!returns)
            {
                _errors.Add(new CompilerError($"Not all code paths return a value in function {context.ID().GetText()}", context.Start.Line, context.Start.Column));
            }
            return returns;
        }

        public override bool VisitState(SctParser.StateContext context)
        {
            var returns = context.statement_list().Accept(this);
            if (!returns)
            {
                _errors.Add(new CompilerError($"Not all code paths return a value in state {context.ID().GetText()}", context.Start.Line, context.Start.Column));
            }
            return returns;
        }

        public override bool VisitStatement_list(SctParser.Statement_listContext context)
        {
            return context.statement().Any(statement => statement.Accept(this));
        }

        public override bool VisitIf(SctParser.IfContext context)
        {
            var returns = context.statement_list().Accept(this);
            if (context.elseif() is not null)
            {
                return returns && context.elseif().Accept(this);
            }
            else if (context.@else() is not null)
            {
                return returns && context.@else().Accept(this);
            }
            return false;
        }

        public override bool VisitElseif(SctParser.ElseifContext context)
        {
            var returns = context.statement_list().Accept(this);
            if (context.elseif() is not null)
            {
                return returns && context.elseif().Accept(this);
            }
            else if (context.@else() is not null)
            {
                return returns && context.@else().Accept(this);
            }
            return false;
        }

        public override bool VisitElse(SctParser.ElseContext context) => context.statement_list().Accept(this);

        public override bool VisitReturn(SctParser.ReturnContext context) => true;

        public override bool VisitEnter(SctParser.EnterContext context) => true;

        public override bool VisitExit(SctParser.ExitContext context) => true;

        public override bool VisitDestroy(SctParser.DestroyContext context) => true;
    }
}

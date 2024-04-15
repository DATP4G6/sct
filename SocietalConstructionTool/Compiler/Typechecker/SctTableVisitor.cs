using Antlr4.Runtime.Misc;

namespace Sct.Compiler.Typechecker
{
    public class SctTableVisitor : SctBaseVisitor<SctType>, IErrorReporter
    {
        private CTable? InternalCTable { get; set; }
        public CTable CTable => InternalCTable ?? _ctableBuilder.BuildCtable();

        private readonly List<CompilerError> _errors = new();
        public IEnumerable<CompilerError> Errors => _errors;
        private readonly CTableBuilder _ctableBuilder = new();

        public override SctType VisitStart([NotNull] SctParser.StartContext context)
        {
            _ = base.VisitStart(context);

            InternalCTable = _ctableBuilder.BuildCtable();

            var setupType = InternalCTable.GlobalClass.LookupFunctionType("Setup");
            if (setupType is null)
            {
                _errors.Add(new CompilerError("No setup function found"));
            }
            else if (setupType.ReturnType != TypeTable.Void || setupType.ParameterTypes.Count != 0)
            {
                _errors.Add(new CompilerError("Setup function must return void and take no arguments"));
            }
            return TypeTable.None;
        }

        public override SctType VisitClass_def([NotNull] SctParser.Class_defContext context)
        {
            string className = context.ID().GetText();

            if (!_ctableBuilder.TryStartClass(className))
            {
                _errors.Add(new CompilerError($"ID {className} already exists", context.Start.Line, context.Start.Column));
            }

            foreach (var (id, type) in context.args_def().ID().Zip(context.args_def().type()))
            {
                if (!_ctableBuilder.TryAddField(id.GetText(), type.Accept(this)))
                {
                    _errors.Add(new CompilerError($"ID {id.GetText()} already exists", id.Symbol.Line, id.Symbol.Column));
                }

            }

            _ = base.VisitClass_def(context);

            _ctableBuilder.FinishClass();

            return TypeTable.None;
        }

        public override SctType VisitFunction([NotNull] SctParser.FunctionContext context)
        {
            var type = context.type().Accept(this);
            var argsTypes = context.args_def().type().Select(arg => arg.Accept(this)).ToList();

            var functionType = new FunctionType(type, argsTypes);

            if (!_ctableBuilder.TryAddFunction(context.ID().GetText(), functionType))
            {
                _errors.Add(new CompilerError($"ID {context.ID().GetText()} already exists", context.Start.Line, context.Start.Column));
            }

            return TypeTable.None;
        }

        public override SctType VisitType([NotNull] SctParser.TypeContext context)
        {
            var type = TypeTable.GetType(context.GetText());
            if (type is null)
            {
                _errors.Add(new CompilerError($"Type {context.GetText()} does not exist", context.Start.Line, context.Start.Column));
            }
            type ??= TypeTable.Int;

            return type;
        }

        public override SctType VisitState([NotNull] SctParser.StateContext context)
        {
            if (!_ctableBuilder.TryAddState(context.ID().GetText()))
            {
                _errors.Add(new CompilerError($"ID {context.ID().GetText()} already exists", context.Start.Line, context.Start.Column));
            }
            return TypeTable.None;
        }

        public override SctType VisitDecorator([NotNull] SctParser.DecoratorContext context)
        {
            if (!_ctableBuilder.TryAddDecorator(context.ID().GetText()))
            {
                _errors.Add(new CompilerError($"ID {context.ID().GetText()} already exists", context.Start.Line, context.Start.Column));
            }
            return TypeTable.None;
        }
    }
}

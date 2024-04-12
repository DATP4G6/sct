using Antlr4.Runtime.Misc;

namespace Sct.Compiler.Typechecker
{
    public class SctTableVisitor(CTableBuilder cTableBuilder) : SctBaseVisitor<SctType>, IErrorReporter
    {
        public CTable? Ctable { get; private set; }

        private readonly List<CompilerError> _errors = new();
        public IEnumerable<CompilerError> Errors => _errors;
        private readonly CTableBuilder _ctableBuilder = cTableBuilder;

        public override SctType VisitStart([NotNull] SctParser.StartContext context)
        {
            _ = base.VisitStart(context);

            // This spot used to check if there was a setup function and if it was of the correct type.
            // This can no longer be done here, as the setup function can be defined in any file.
            // And the TableVisitor is run on each file separately.
            // TODO: Find another way to check this.

            return TypeTable.None;
        }

        public override SctType VisitClass_def([NotNull] SctParser.Class_defContext context)
        {
            string className = context.ID().GetText();

            if (!_ctableBuilder.StartClass(className))
            {
                _errors.Add(new CompilerError($"ID {className} already exists", context.Start.Line, context.Start.Column));
            }

            foreach (var (id, type) in context.args_def().ID().Zip(context.args_def().type()))
            {
                if (!_ctableBuilder.AddField(id.GetText(), type.Accept(this)))
                {
                    _errors.Add(new CompilerError($"ID {id.GetText()} already exists", id.Symbol.Line, id.Symbol.Column));
                }

            }

            _ = base.VisitClass_def(context);

            _ = _ctableBuilder.FinishClass();

            return TypeTable.None;
        }

        public override SctType VisitFunction([NotNull] SctParser.FunctionContext context)
        {
            var type = context.type().Accept(this);
            var argsTypes = context.args_def().type().Select(arg => arg.Accept(this)).ToList();

            var functionType = new FunctionType(type, argsTypes);

            if (!_ctableBuilder.AddFunction(context.ID().GetText(), functionType))
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
            if (!_ctableBuilder.AddState(context.ID().GetText()))
            {
                _errors.Add(new CompilerError($"ID {context.ID().GetText()} already exists", context.Start.Line, context.Start.Column));
            }
            return TypeTable.None;
        }

        public override SctType VisitDecorator([NotNull] SctParser.DecoratorContext context)
        {
            if (!_ctableBuilder.AddDecorator(context.ID().GetText()))
            {
                _errors.Add(new CompilerError($"ID {context.ID().GetText()} already exists", context.Start.Line, context.Start.Column));
            }
            return TypeTable.None;
        }
    }
}

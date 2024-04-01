using Antlr4.Runtime.Misc;

namespace Sct.Compiler
{
    public class SctTableVisitor : SctBaseVisitor<SctType>, IErrorReporter
    {

        public Ctable? Ctable { get; private set; }

        private readonly List<CompilerError> _errors = new();
        public IEnumerable<CompilerError> Errors => _errors;
        private readonly CtableBuilder _ctableBuilder = new CtableBuilder();
        private readonly TypeTable _typeTable = new TypeTable();

        public override SctType VisitStart([NotNull] SctParser.StartContext context)
        {
            _ = base.VisitStart(context);

            Ctable = _ctableBuilder.BuildCtable();
            return _typeTable.Void;
        }

        public override SctType VisitClass_def([NotNull] SctParser.Class_defContext context)
        {
            string className = context.ID().GetText();
            _ = _ctableBuilder.StartClass(className);

            _ = base.VisitClass_def(context);

            _ = _ctableBuilder.FinishClass();

            return _typeTable.Void;
        }

        public override SctType VisitFunction([NotNull] SctParser.FunctionContext context)
        {

            var type = _typeTable.GetType(context.type().GetText());
            var argsTypes = context.args_def().type().Select(arg => arg.Accept(this)).ToList();

            if (type is null)
            {
                _errors.Add(new CompilerError($"Type {context.type().GetText()} does not exist"));
            }
            type ??= _typeTable.Int;
            FunctionType functionType = new FunctionType(type, argsTypes);

            _ = _ctableBuilder.AddFunction(context.ID().GetText(), functionType);

            return type;
        }

        public override SctType VisitType([NotNull] SctParser.TypeContext context)
        {
            var type = _typeTable.GetType(context.GetText());
            if (type is null)
            {
                _errors.Add(new CompilerError($"Type {context.GetText()} does not exist"));
            }
            type ??= _typeTable.Int;

            return type;
        }

        public override SctType VisitState([NotNull] SctParser.StateContext context)
        {
            if (!_ctableBuilder.AddState(context.ID().GetText()))
            {
                _errors.Add(new CompilerError($"ID {context.ID().GetText()} already exists"));
            }
            return _typeTable.Void;
        }

        public override SctType VisitDecorator([NotNull] SctParser.DecoratorContext context)
        {
            if (!_ctableBuilder.AddDecorator(context.ID().GetText()))
            {
                _errors.Add(new CompilerError($"ID {context.ID().GetText()} already exists"));
            }
            return _typeTable.Void;
        }
    }
}

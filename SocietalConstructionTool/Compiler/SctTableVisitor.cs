using Antlr4.Runtime.Misc;

namespace Sct.Compiler
{
    public class SctTableVisitor : SctBaseVisitor<SctType>, IErrorReporter
    {

        public Ctable? Ctable { get; private set; }

        private readonly List<CompilerError> _errors = new();
        public IEnumerable<CompilerError> Errors => _errors;
        private readonly CtableBuilder _ctableBuilder = new CtableBuilder();
        public override SctType VisitStart([NotNull] SctParser.StartContext context)
        {
            _ = base.VisitStart(context);

            Ctable = _ctableBuilder.BuildCtable();

            var setupType = Ctable.GetGlobalContent().LookupFunctionType("Setup");
            if (setupType is null)
            {
                _errors.Add(new CompilerError("No setup function found"));
            }
            else if (setupType.ReturnType != TypeTable.Void || setupType.ParameterTypes.Count != 0)
            {
                _errors.Add(new CompilerError("Setup function must return void and take no arguments"));
            }
            return TypeTable.Void;
        }

        public override SctType VisitClass_def([NotNull] SctParser.Class_defContext context)
        {
            string className = context.ID().GetText();

            _ = _ctableBuilder.StartClass(className);

            foreach (var (id, type) in context.args_def().ID().Zip(context.args_def().type()))
            {
                _ctableBuilder.AddField(id.GetText(), TypeTable.GetType(type.GetText())!);

            }


            _ = base.VisitClass_def(context);

            _ = _ctableBuilder.FinishClass();

            return TypeTable.Void;
        }

        public override SctType VisitFunction([NotNull] SctParser.FunctionContext context)
        {
            var type = TypeTable.GetType(context.type().GetText());
            var argsTypes = context.args_def().type().Select(arg => arg.Accept(this)).ToList();

            if (type is null)
            {
                _errors.Add(new CompilerError($"Type {context.GetText()} does not exist", context.Start.Line, context.Start.Column));
            }
            type ??= TypeTable.Int;

            FunctionType functionType = new FunctionType(type, argsTypes);

            _ = _ctableBuilder.AddFunction(context.ID().GetText(), functionType);

            return type;
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
            return TypeTable.Void;
        }

        public override SctType VisitDecorator([NotNull] SctParser.DecoratorContext context)
        {
            if (!_ctableBuilder.AddDecorator(context.ID().GetText()))
            {
                _errors.Add(new CompilerError($"ID {context.ID().GetText()} already exists", context.Start.Line, context.Start.Column));
            }
            return TypeTable.Void;
        }
    }
}

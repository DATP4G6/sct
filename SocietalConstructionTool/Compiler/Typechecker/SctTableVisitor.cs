using Antlr4.Runtime.Misc;

namespace Sct.Compiler.Typechecker
{
    public class SctTableVisitor : SctBaseVisitor<SctType>, IErrorReporter
    {

        public Ctable? Ctable { get; private set; }

        private readonly List<CompilerError> _errors = new();
        public IEnumerable<CompilerError> Errors => _errors;
        private readonly CtableBuilder _ctableBuilder = new CtableBuilder();

        private SctType GetType(SctParser.TypeContext context)
        {
            var type = TypeTable.GetType(context.GetText());
            if (type is null)
            {
                _errors.Add(new CompilerError($"Type {context.GetText()} does not exist", context.Start.Line, context.Start.Column));
            }
            type ??= TypeTable.Int;

            return type;
        }

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

            if (!_ctableBuilder.StartClass(className))
            {
                _errors.Add(new CompilerError($"ID {className} already exists", context.Start.Line, context.Start.Column));
            }

            foreach (var (id, type) in context.args_def().ID().Zip(context.args_def().type()))
            {
                if (!_ctableBuilder.AddField(id.GetText(), GetType(type)))
                {
                    _errors.Add(new CompilerError($"ID {id.GetText()} already exists", id.Symbol.Line, id.Symbol.Column));
                }

            }


            _ = base.VisitClass_def(context);

            _ = _ctableBuilder.FinishClass();

            return TypeTable.Void;
        }

        public override SctType VisitFunction([NotNull] SctParser.FunctionContext context)
        {
            var type = GetType(context.type());
            var argsTypes = context.args_def().type().Select(arg => arg.Accept(this)).ToList();

            FunctionType functionType = new FunctionType(type, argsTypes);

            if(!_ctableBuilder.AddFunction(context.ID().GetText(), functionType)){
                _errors.Add(new CompilerError($"ID {context.ID().GetText()} already exists", context.Start.Line, context.Start.Column));
            }

            return type;
        }

        public override SctType VisitType([NotNull] SctParser.TypeContext context)
        {
            return GetType(context);
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

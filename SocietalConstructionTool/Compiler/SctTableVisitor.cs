using Antlr4.Runtime.Misc;

namespace Sct.Compiler
{
    public class SctTableVisitor : SctBaseVisitor<SctType>
    {

        public Ctable? Ctable { get; private set; }
        private readonly CtableBuilder _ctableBuilder = new CtableBuilder();
        private readonly TypeTable _typeTable = new TypeTable();
        private readonly List<string> _errors = new List<string>();

        public override SctType VisitStart([NotNull] SctParser.StartContext context)
        {

            foreach (var classDef in context.class_def())
            {
                _ = classDef.Accept(this);
            }

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
                _errors.Add($"Type {context.type().GetText()} does not exist");
            }
            type ??= _typeTable.Int;
            FunctionType functionType = new FunctionType(type, argsTypes);

            return type;
        }
    }
}

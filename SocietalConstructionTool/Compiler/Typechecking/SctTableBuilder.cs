using Antlr4.Runtime.Misc;

namespace Sct.Compiler.Typechecking
{
    public class SctTableBuilder : SctBaseListener
    {
        private SctFTable? _fTable;
        public SctFTable FTable => _fTable ?? throw new NullReferenceException("FTable hasn't been built yet");
        private SctFTable? _sTable;
        public SctFTable STable => _sTable ?? throw new NullReferenceException("STable hasn't been built yet");
        private SctFTable? _dTable;
        public SctFTable DTable => _dTable ?? throw new NullReferenceException("DTable hasn't been built yet");
        private SctCTable? _cTable;
        public SctCTable CTable => _cTable ?? throw new NullReferenceException("CTable hasn't been built yet");
        public List<string> Errors { get; private set; } = new();
        private readonly SctFTable.SctFTableBuilder _fTableBuilder = new();
        private readonly SctFTable.SctFTableBuilder _sTableBuilder = new();
        private readonly SctFTable.SctFTableBuilder _dTableBuilder = new();
        private readonly SctCTable.SctCTableBuilder _cTableBuilder = new();
        public override void EnterStart([NotNull] SctParser.StartContext context)
        {
            _ = _fTableBuilder.AddFunction("count", TypeTable.Int, TypeTable.Predicate);
            _ = _fTableBuilder.AddFunction("exists", TypeTable.Int, TypeTable.Predicate);
            _ = _fTableBuilder.AddFunction("rand", TypeTable.Float);
            _ = _fTableBuilder.AddFunction("seed", TypeTable.Void, TypeTable.Float);
        }
        public override void ExitStart([NotNull] SctParser.StartContext context)
        {
            _fTable = _fTableBuilder.Build();
            _sTable = _sTableBuilder.Build();
            _dTable = _dTableBuilder.Build();
            _cTable = _cTableBuilder.Build();
        }
        public override void EnterClass_def([NotNull] SctParser.Class_defContext context)
        {
            _ = _fTableBuilder.EnterScope();
            _ = _sTableBuilder.EnterScope();
            _ = _dTableBuilder.EnterScope();
        }
        public override void ExitClass_def([NotNull] SctParser.Class_defContext context)
        {
            context.args_def().ID().Zip(
                context.args_def().type(),
                (id, type) => (id.GetText(), type.GetText())).ToList()
                    .ForEach(x =>
                    {
                        if (!_cTableBuilder.AddField(x.Item1, TypeTable.GetType(x.Item2)))
                        {
                            Errors.Add($"Field {x.Item1} already exists in class {context.ID}");
                        }
                    });

            if (!_cTableBuilder.FinishClass(context.ID().GetText()))
            {
                Errors.Add($"Class {context.ID} already exists");
            }
        }
        public override void ExitFunction([NotNull] SctParser.FunctionContext context) => _fTableBuilder.AddFunction(
            context.ID().GetText(),
            TypeTable.GetType(context.type().GetText()),
            context.args_def().type().Select(x => TypeTable.GetType(x.GetText())).ToArray()
            );
        public override void ExitDecorator([NotNull] SctParser.DecoratorContext context) => _dTableBuilder.AddFunction(context.ID().GetText(), TypeTable.Void);
        public override void ExitState([NotNull] SctParser.StateContext context)
        {
            _ = _sTableBuilder.AddFunction(context.ID().GetText(), TypeTable.Void);
            if (!_cTableBuilder.AddState(context.ID().GetText()))
            {
                Errors.Add($"State {context.ID} already exists");
            }
        }
    }
}

using Sct.Compiler.Syntax;

namespace Sct.Compiler.Typechecker
{
    public class SctTableBuilder(CTableBuilder cTableBuilder) : SctBaseSyntaxVisitor<Syntax.SctType>
    {
        private readonly List<CompilerError> _errors = [];
        public IEnumerable<CompilerError> Errors => _errors;
        private readonly CTableBuilder _ctableBuilder = cTableBuilder;

        protected override Syntax.SctType DefaultResult => Syntax.SctType.Ok;

        public override Syntax.SctType Visit(SctClassSyntax node)
        {
            string className = node.Id;

            if (!_ctableBuilder.TryStartClass(className))
            {
                _errors.Add(new CompilerError($"ID {className} already exists", node.Context));
            }

            foreach (var parameter in node.Parameters)
            {
                var id = parameter.Id;
                var type = parameter.Type;
                if (!_ctableBuilder.TryAddField(id, type.Accept(this)))
                {
                    _errors.Add(new CompilerError($"ID {id} already exists", node.Context));
                }

            }

            // We need to call `ToList` because select is lazy, so if we simply discard the result, then the body is never run
            // Converting to a list forces the enumerable to be evaluated
            _ = node.Body.Select(m => m.Accept(this)).ToList();

            _ctableBuilder.FinishClass();

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctFunctionSyntax node)
        {
            var returnType = node.ReturnType.Accept(this);
            var argsTypes = node.Parameters.Select(arg => arg.Type.Accept(this)).ToList();

            var functionType = new FunctionType(returnType, argsTypes);

            if (!_ctableBuilder.TryAddFunction(node.Id, functionType))
            {
                _errors.Add(new CompilerError($"ID {node.Id} already exists", node.Context));
            }

            _ = node.Block.Accept(this);

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctTypeSyntax node) => node.Type;

        public override Syntax.SctType Visit(SctStateSyntax node)
        {
            if (!_ctableBuilder.TryAddState(node.Id))
            {
                _errors.Add(new CompilerError($"ID {node.Id} already exists", node.Context));
            }
            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctDecoratorSyntax node)
        {
            if (!_ctableBuilder.TryAddDecorator(node.Id))
            {
                _errors.Add(new CompilerError($"ID {node.Id} already exists", node.Context));
            }
            return Syntax.SctType.Ok;
        }
    }
}

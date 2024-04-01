
using Antlr4.Runtime.Misc;

using Sct.Compiler.Exceptions;

namespace Sct.Compiler
{
    public class SctTypeChecker : SctBaseVisitor<SctType>, IErrorReporter
    {
        private readonly TypeTable _typeTable = new();


        private readonly Ctable _ctable;

        private string _currentClass = "Global";
        // private ClassContent _currentClass

        private readonly List<CompilerError> _errors = new();
        public IEnumerable<CompilerError> Errors => _errors;
        public SctTypeChecker(Ctable ctable)
        {
            _ctable = ctable;
        }

        public override SctType VisitStart([NotNull] SctParser.StartContext context)
        {
            _ = base.VisitStart(context);

            return _typeTable.Void;
        }

        public override SctType VisitVariableDeclaration([NotNull] SctParser.VariableDeclarationContext context)
        {

            string typeName = context.type().GetText();
            SctType type = _typeTable.GetType(typeName) ?? throw new InvalidTypeException($"Type {typeName} does not exist");
            if (type == _typeTable.GetType("Predicate"))
            {
                // Errors.Add(new InvalidTypeException("Variable cannot have a Predicate type"));
            }
            SctType expressionType = Visit(context.expression()); // This should call our overridden VisitExpression method with the expression context.
            if (type != expressionType)
            {
                //Errors.Add(new InvalidTypeException($"Type mismatch: {type} != {expressionType}"));
            }
            return type;
        }

        public override SctType VisitClass_def([NotNull] SctParser.Class_defContext context)
        {
            _currentClass = context.ID().GetText();
            _ = base.VisitClass_def(context);
            _currentClass = "Global";
            return _typeTable.Void;
        }

        public override SctType VisitEnter([NotNull] SctParser.EnterContext context)
        {
            var stateName = context.ID().GetText();
            if (!_ctable.StateExists(_currentClass, stateName))
            {
                _errors.Add(new CompilerError($"State {stateName} does not exist in class {_currentClass}"));
            }
            return base.VisitEnter(context);
        }

        public override SctType VisitDecorator([NotNull] SctParser.DecoratorContext context)
        {
            var decoratorName = context.ID().GetText();
            if (!_ctable.DecoratorExists(_currentClass, decoratorName))
            {
                _errors.Add(new CompilerError($"Decorator {decoratorName} does not exist in class {_currentClass}"));
            }
            return base.VisitDecorator(context);
        }

        public override SctType VisitCallExpression([NotNull] SctParser.CallExpressionContext context)
        {
            var functionName = context.ID().GetText();
            var functionType = _ctable.GetFunctionType(_currentClass, functionName);
            if (functionType is null)
            {
                _errors.Add(new CompilerError($"Function {functionName} does not exist"));
            }
            // TODO: CHECK ARGUMENTS AND RETURN TYPE MATCH YES :)))
            return base.VisitCallExpression(context);
        }

        public override SctType VisitReturn([NotNull] SctParser.ReturnContext context)
        {
            return base.VisitReturn(context);
        }

        public override SctType VisitBinaryExpression([NotNull] SctParser.BinaryExpressionContext context)
        {
            var leftType = context.expression(0).Accept(this);
            var rightType = context.expression(1).Accept(this);
            if (!_typeTable.TypeIsNumeric(leftType) || !_typeTable.TypeIsNumeric(rightType))
            {
                _errors.Add(new CompilerError("Binary expression must have numeric types"));
                leftType = _typeTable.Int;
                rightType = _typeTable.Int;
            }

            return (leftType == rightType) ? leftType : _typeTable.Float;
        }

        public override SctType VisitLiteral([NotNull] SctParser.LiteralContext context)
        {
            if (context.INT() is not null)
            {
                return _typeTable.Int;
            }
            else if (context.FLOAT() is not null)
            {
                return _typeTable.Float;
            }
            else
            {
                _errors.Add(new CompilerError("Literal type not recognized, must be int or float."));
                return _typeTable.Int;
            }
        }
    }
}

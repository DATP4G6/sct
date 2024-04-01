using Antlr4.Runtime.Misc;

using Sct.Compiler.Exceptions;

namespace Sct.Compiler
{
    public class SctTypeChecker : SctBaseVisitor<SctType>, IErrorReporter
    {
        private readonly TypeTable _typeTable = new();

        private readonly Ctable _ctable;

        private ClassContent _currentClass;

        private readonly List<CompilerError> _errors = new();
        public IEnumerable<CompilerError> Errors => _errors;
        public SctTypeChecker(Ctable ctable)
        {
            _ctable = ctable;
            _currentClass = _ctable.GetGlobalContent();
        }

        public FunctionType GetFunctionType(string functionName)
        {
            if (_currentClass.LookupFunctionType(functionName) is FunctionType functionType)
            {
                return functionType;
            }
            else if (_ctable.GetGlobalContent().LookupFunctionType(functionName) is FunctionType globalFunctionType)
            {
                return globalFunctionType;
            }
            _errors.Add(new CompilerError($"Function {functionName} does not exist"));
            return new FunctionType(_typeTable.Void, new List<SctType>());
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
                _errors.Add(new CompilerError("Variable cannot have a Predicate type"));
            }
            SctType expressionType = Visit(context.expression()); // This should call our overridden VisitExpression method with the expression context.
            if (type != expressionType)
            {
                _errors.Add(new CompilerError($"Type mismatch: {type} != {expressionType}"));
            }

            if (!_currentClass.AddVariable(context.ID().GetText(), type))
            {
                _errors.Add(new CompilerError($"Variable {context.ID().GetText()} already exists"));
                return _currentClass.LookupVariable(context.ID().GetText());
            }

            return type;
        }

        public override SctType VisitClass_def([NotNull] SctParser.Class_defContext context)
        {
            _currentClass = _ctable.GetClassContent(context.ID().GetText());
            _ = base.VisitClass_def(context);
            _currentClass = _ctable.GetGlobalContent();
            return _typeTable.Void;
        }

        public override SctType VisitEnter([NotNull] SctParser.EnterContext context)
        {
            var stateName = context.ID().GetText();
            if (_currentClass.LookupState(stateName) is null)
            {
                _errors.Add(new CompilerError($"State {stateName} does not exist in class {_currentClass}"));
            }
            return base.VisitEnter(context);
        }

        public override SctType VisitDecorator([NotNull] SctParser.DecoratorContext context)
        {
            var decoratorName = context.ID().GetText();
            if (_currentClass.LookupDecorator(decoratorName) is null)
            {
                _errors.Add(new CompilerError($"Decorator {decoratorName} does not exist in class {_currentClass}"));
            }
            return base.VisitDecorator(context);
        }

        public override SctType VisitCallExpression([NotNull] SctParser.CallExpressionContext context)
        {
            // var functionName = context.ID().GetText();
            // var functionType = GetFunctionType(functionName);
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

        // This method is unnecessary, as we can just return the type of the expression inside the parentheses.
        public override SctType VisitParenthesisExpression([NotNull] SctParser.ParenthesisExpressionContext context)
        {
            return context.expression().Accept(this);
        }

        public override SctType VisitAssignment([NotNull] SctParser.AssignmentContext context)
        {
            var variableName = context.ID().GetText();
            var variableType = _currentClass.LookupVariable(variableName);
            var expressionType = context.expression().Accept(this);

            if (variableType is null)
            {
                _errors.Add(new CompilerError($"Variable {variableName} does not exist"));
                return _typeTable.Int;
            }

            if (variableType != expressionType)
            {
                _errors.Add(new CompilerError($"Type mismatch: {variableType} != {expressionType}"));
            }

            return variableType;
        }

        public override SctType VisitBooleanExpression([NotNull] SctParser.BooleanExpressionContext context)
        {
            var leftType = context.expression(0).Accept(this);
            var rightType = context.expression(1).Accept(this);
            if (leftType != _typeTable.Int || rightType != _typeTable.Int)
            {
                _errors.Add(new CompilerError("Boolean expression must have integer types"));
            }
            return _typeTable.Int;
        }
    }
}

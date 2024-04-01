using Antlr4.Runtime.Misc;

using Sct.Compiler.Exceptions;

namespace Sct.Compiler
{
    public class SctTypeChecker : SctBaseVisitor<SctType>, IErrorReporter
    {
        private readonly TypeTable _typeTable = new();

        private readonly Ctable _ctable;

        private ClassContent _currentClass;

        private FunctionType currentFunctionType;

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
            if (type == _typeTable.Predicate || type == _typeTable.Void)
            {
                _errors.Add(new CompilerError($"Variable cannot be of type :{type}"));
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

        public override SctType VisitFunction([NotNull] SctParser.FunctionContext context)
        {
            var functionName = context.ID().GetText();
            currentFunctionType = GetFunctionType(functionName);
            return base.VisitFunction(context);
        }

        public override SctType VisitCallExpression([NotNull] SctParser.CallExpressionContext context)
        {
            var functionName = context.ID().GetText();
            var functionParamTypes = GetFunctionType(functionName).ParameterTypes;
            var argumentTypes = context.args_call().expression().Select(expression => expression.Accept(this)).ToList();
            if (functionParamTypes.Count == argumentTypes.Count)
            {
                foreach (var (functionParamType, argumentType) in functionParamTypes.Zip(argumentTypes))
                {
                    if (functionParamType != argumentType)
                    {
                        _errors.Add(new CompilerError($"Type mismatch: {functionParamType} != {argumentType}"));
                    }
                }
            } else {
                _errors.Add(new CompilerError($"Function {functionName} expected {functionParamTypes.Count} arguments, but {argumentTypes.Count} were provided"));
            }
            return GetFunctionType(functionName).ReturnType;
        }

        public override SctType VisitReturn([NotNull] SctParser.ReturnContext context) {
            var returnType = context.expression().Accept(this);
            var functionReturnType = currentFunctionType.ReturnType;
            if (returnType != functionReturnType)
            {
                _errors.Add(new CompilerError("Return type does not match function return type"));
                returnType = functionReturnType;
            }
            return returnType;
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

        public override SctType VisitTypecastExpression([NotNull] SctParser.TypecastExpressionContext context)
        {
            var targetTypeName = context.type().GetText();
            var targetType = _typeTable.GetType(targetTypeName);
            if (targetType is null)
            {
                _errors.Add(new CompilerError($"Type {targetTypeName} does not exist"));
                return _typeTable.Int;
            }

            if (targetType == _typeTable.Predicate)
            {
                _errors.Add(new CompilerError($"Typecast cannot have a {_typeTable.Predicate} type"));
            }

            var expressionType = context.expression().Accept(this);
            if (!_typeTable.IsTypeCastable(expressionType, targetType))
            {
                _errors.Add(new CompilerError($"Type mismatch: Cannot typecast from {expressionType} to {targetType}."));
            }
            return targetType;
        }

        public override SctType VisitAgent_predicate([NotNull] SctParser.Agent_predicateContext context)
        {

            var targetAgent = context.ID(0).GetText();

            if (_ctable.GetClassContent(targetAgent) is null)
            {
                _errors.Add(new CompilerError($"Agent {targetAgent} does not exist"));
                return _typeTable.Predicate;
            }

            if (context.QUESTION() is null && _ctable.GetClassContent(targetAgent).LookupState(context.ID(1).GetText()) is null)
            {
                _errors.Add(new CompilerError($"State {context.ID(1).GetText()} does not exist in agent {targetAgent}"));
                return _typeTable.Predicate;
            }

            var targetAgentFields = _ctable.GetClassContent(targetAgent);
            var agentArgumentIds = context.args_agent().ID();

            var agentArgs = new Dictionary<string, SctType>();

            foreach (var id in agentArgumentIds)
            {
                if (targetAgentFields.LookupVariable(id.GetText()) is null)
                {
                    _errors.Add(new CompilerError($"Variable {id.GetText()} does not exist in agent {targetAgent}"));
                    agentArgs.Add(id.GetText(), _typeTable.Int);
                }

                if (agentArgs.TryAdd(id.GetText(), context.args_agent().expression(agentArgs.Count).Accept(this)))
                {
                    _errors.Add(new CompilerError($"Duplicate argument {id.GetText()} in agent predicate."));
                }
            }

            foreach (var arg in agentArgs)
            {
                if (targetAgentFields.LookupVariable(arg.Key) != arg.Value)
                {
                    _errors.Add(new CompilerError($"Type mismatch: {targetAgentFields.LookupVariable(arg.Key)} != {arg.Value}. Expression in predicate does not match field type in target agent."));
                }
            }

            return _typeTable.Predicate;
        }

        public override SctType VisitStatement_list([NotNull] SctParser.Statement_listContext context)
        {
            _currentClass.Vtable.EnterScope();
            _ = base.VisitStatement_list(context);
            _currentClass.Vtable.ExitScope();
            return _typeTable.Void;
        }
    }
}

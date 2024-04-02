using Antlr4.Runtime.Misc;

using Sct.Compiler.Exceptions;

namespace Sct.Compiler
{
    public class SctTypeChecker : SctBaseVisitor<SctType>, IErrorReporter
    {

        private readonly Ctable _ctable;
        private readonly Vtable _vtable = new();

        private ClassContent _currentClass;

        private FunctionType? _currentFunctionType;

        private readonly List<CompilerError> _errors = new();
        public IEnumerable<CompilerError> Errors => _errors;
        public SctTypeChecker(Ctable ctable)
        {
            _ctable = ctable;
            _currentClass = _ctable.GetGlobalContent();
        }

        private FunctionType GetFunctionType(string functionName)
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
            return new FunctionType(TypeTable.Void, new List<SctType>());
        }

        private SctType? GetCompatibleType(SctType left, SctType right)
        {
            if (left == right)
            {
                return left;
            } else if (left == TypeTable.Float && right == TypeTable.Int)
            {
                return TypeTable.Float;
            }
            return null;
        }

        public override SctType VisitStart([NotNull] SctParser.StartContext context)
        {
            _ = base.VisitStart(context);

            return TypeTable.Void;
        }

        public override SctType VisitVariableDeclaration([NotNull] SctParser.VariableDeclarationContext context)
        {
            string typeName = context.type().GetText();
            var type = TypeTable.GetType(typeName);

            if (type is null)
            {
                _errors.Add(new CompilerError($"Type {typeName} does not exist"));
                return TypeTable.Int;
            }
            // TODO: Maybe add predicate later :)
            if (type == TypeTable.Predicate || type == TypeTable.Void)
            {
                _errors.Add(new CompilerError($"Variable cannot be of type :{type}"));
            }

            SctType expressionType = context.expression().Accept(this);
            if (GetCompatibleType(type, expressionType) is null)
            {
                _errors.Add(new CompilerError($"Type mismatch: {type} != {expressionType}"));
            }

            if (!_vtable.AddEntry(context.ID().GetText(), type))
            {
                _errors.Add(new CompilerError($"Variable {context.ID().GetText()} already exists"));
                return _vtable.Lookup(context.ID().GetText());
            }

            return type;
        }

        public override SctType VisitClass_def([NotNull] SctParser.Class_defContext context)
        {
            _currentClass = _ctable.GetClassContent(context.ID().GetText());
            _vtable.EnterScope();

            foreach (var (id, type) in context.args_def().ID().Zip(context.args_def().type()))
            {
                _ = _vtable.AddEntry(id.GetText(), TypeTable.GetType(type.GetText())!);
            }

            _ = base.VisitClass_def(context);
            _currentClass = _ctable.GetGlobalContent();
            _vtable.ExitScope();
            return TypeTable.Void;
        }

        public override SctType VisitIDExpression([NotNull] SctParser.IDExpressionContext context)
        {
            var variableName = context.ID().GetText();
            var variableType = _vtable.Lookup(variableName);
            if (variableType is null)
            {
                _errors.Add(new CompilerError($"Variable {variableName} does not exist", context.Start.Line, context.Start.Column));
                return TypeTable.Int;
            }
            return variableType;
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
            _currentFunctionType = GetFunctionType(functionName);
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
                    if (GetCompatibleType(functionParamType, argumentType) is null)
                    {
                        _errors.Add(new CompilerError($"Type mismatch: {functionParamType.TargetType} != {argumentType.TargetType}"));
                    }
                }
            }
            else
            {
                _errors.Add(new CompilerError($"Function {functionName} expected {functionParamTypes.Count} arguments, but {argumentTypes.Count} were provided"));
            }
            return GetFunctionType(functionName).ReturnType;
        }

        public override SctType VisitReturn([NotNull] SctParser.ReturnContext context)
        {

            if (context.expression() is null && _currentFunctionType!.ReturnType != TypeTable.Void)
            {
                _errors.Add(new CompilerError("Return type does not match function return type, only void functions can return nothing"));
                return _currentFunctionType!.ReturnType;
            }
            // TODO: Fix these if statements.
            if (context.expression() is null)
            {
                return TypeTable.Void;
            }
            var returnType = context.expression().Accept(this);
            var functionReturnType = _currentFunctionType!.ReturnType;
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
            if (!TypeTable.TypeIsNumeric(leftType) || !TypeTable.TypeIsNumeric(rightType))
            {
                _errors.Add(new CompilerError("Binary expression must have numeric types"));
                leftType = TypeTable.Int;
                rightType = TypeTable.Int;
            }

            return (leftType == rightType) ? leftType : TypeTable.Float; // If we have two different types (int and float), we return float.
        }

        public override SctType VisitLiteral([NotNull] SctParser.LiteralContext context)
        {
            if (context.INT() is not null)
            {
                return TypeTable.Int;
            }
            else if (context.FLOAT() is not null)
            {
                return TypeTable.Float;
            }
            else
            {
                _errors.Add(new CompilerError("Literal type not recognized, must be int or float."));
                return TypeTable.Int;
            }
        }

        public override SctType VisitAssignment([NotNull] SctParser.AssignmentContext context)
        {
            var variableName = context.ID().GetText();
            var variableType = _vtable.Lookup(variableName);
            var expressionType = context.expression().Accept(this);

            if (variableType is null)
            {
                _errors.Add(new CompilerError($"Variable {variableName} does not exist"));
                return TypeTable.Int;
            }

            if (GetCompatibleType(variableType, expressionType) is null)
            {
                _errors.Add(new CompilerError($"Type mismatch: {variableType.TargetType} != {expressionType.TargetType}"));
            }

            return variableType;
        }

        public override SctType VisitBooleanExpression([NotNull] SctParser.BooleanExpressionContext context)
        {
            var leftType = context.expression(0).Accept(this);
            var rightType = context.expression(1).Accept(this);
            if (!TypeTable.TypeIsNumeric(leftType) || !TypeTable.TypeIsNumeric(rightType))
            {
                _errors.Add(new CompilerError("Boolean expression must be numeric types"));
            }
            return TypeTable.Int;
        }

        public override SctType VisitTypecastExpression([NotNull] SctParser.TypecastExpressionContext context)
        {
            var targetTypeName = context.type().GetText();
            var targetType = TypeTable.GetType(targetTypeName);
            if (targetType is null)
            {
                _errors.Add(new CompilerError($"Type {targetTypeName} does not exist"));
                return TypeTable.Int;
            }

            var expressionType = context.expression().Accept(this);
            if (!TypeTable.IsTypeCastable(expressionType, targetType))
            {
                _errors.Add(new CompilerError($"Type mismatch: Cannot typecast from {expressionType.TargetType} to {targetType.TargetType}."));
            }
            return targetType;
        }

        public override SctType VisitAgent_predicate([NotNull] SctParser.Agent_predicateContext context)
        {
            var targetAgent = context.ID(0).GetText();

            if (_ctable.GetClassContent(targetAgent) is null)
            {
                _errors.Add(new CompilerError($"Agent {targetAgent} does not exist"));
                return TypeTable.Predicate;
            }

            if (context.QUESTION() is null && _ctable.GetClassContent(targetAgent).LookupState(context.ID(1).GetText()) is null)
            {
                _errors.Add(new CompilerError($"State {context.ID(1).GetText()} does not exist in agent {targetAgent}"));
                return TypeTable.Predicate;
            }

            var targetAgentFields = _ctable.GetClassContent(targetAgent).Fields;
            var agentArgumentIds = context.args_agent().ID();

            var agentArgs = new Dictionary<string, SctType>();

            foreach (var id in agentArgumentIds)
            {
                if (targetAgentFields[id.GetText()] is null)
                {
                    _errors.Add(new CompilerError($"Variable {id.GetText()} does not exist in agent {targetAgent}"));
                    agentArgs.Add(id.GetText(), TypeTable.Int);
                }

                if (!agentArgs.TryAdd(id.GetText(), context.args_agent().expression(agentArgs.Count).Accept(this)))
                {
                    _errors.Add(new CompilerError($"Duplicate argument {id.GetText()} in agent predicate."));
                }
            }

            foreach (var arg in agentArgs)
            {
                if (targetAgentFields[arg.Key] != arg.Value)
                {
                    _errors.Add(new CompilerError($"Type mismatch: {targetAgentFields[arg.Key].TargetType} != {arg.Value.TargetType}. Expression in predicate does not match field type in target agent."));
                }
            }

            return TypeTable.Predicate;
        }

        public override SctType VisitStatement_list([NotNull] SctParser.Statement_listContext context)
        {
            _vtable.EnterScope();
            _ = base.VisitStatement_list(context);
            _vtable.ExitScope();
            return TypeTable.Void;
        }
    }
}

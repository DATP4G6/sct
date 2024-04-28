using Antlr4.Runtime.Misc;

using Sct.Compiler.Exceptions;

namespace Sct.Compiler.Typechecker
{
    public class SctTypeChecker : SctBaseVisitor<SctType>, IErrorReporter
    {
        private readonly CTable _ctable;
        private readonly VTable _vtable = new();

        private ClassContent _currentClass;

        private FunctionType _currentFunctionType = new(TypeTable.Void, []);

        private readonly List<CompilerError> _errors = new();
        public IEnumerable<CompilerError> Errors => _errors;
        public SctTypeChecker(CTable ctable)
        {
            _ctable = ctable;
            _currentClass = _ctable.GlobalClass;
        }

        private FunctionType GetFunctionType(string functionName, int line, int col)
        {
            if (_currentClass.LookupFunctionType(functionName) is FunctionType functionType)
            {
                return functionType;
            }
            else if (_ctable.GlobalClass.LookupFunctionType(functionName) is FunctionType globalFunctionType)
            {
                return globalFunctionType;
            }
            _errors.Add(new CompilerError($"Function '{functionName}' does not exist.", line, col));
            return new FunctionType(TypeTable.Void, []);
        }

        private SctType LookupVariable(string variableName, int line, int col)
        {
            var variableType = _vtable.Lookup(variableName);
            if (variableType is null)
            {
                _errors.Add(new CompilerError($"Variable '{variableName}' does not exist.", line, col));
                return TypeTable.Int;
            }
            return variableType;
        }

        public override SctType VisitStart([NotNull] SctParser.StartContext context)
        {
            _ = base.VisitStart(context);

            return TypeTable.Ok;
        }

        public override SctType VisitParenthesisExpression([NotNull] SctParser.ParenthesisExpressionContext context)
        {

            return context.expression().Accept(this);
        }

        public override SctType VisitVariableDeclaration([NotNull] SctParser.VariableDeclarationContext context)
        {
            var type = context.type().Accept(this);

            if (type == TypeTable.Void)
            {
                _errors.Add(new CompilerError($"Variable cannot be of type {type.TypeName}.", context.Start.Line, context.Start.Column));
            }

            SctType expressionType = context.expression().Accept(this);
            if (TypeTable.GetCompatibleType(type, expressionType) is null)
            {
                _errors.Add(new CompilerError($"Cannot assign {expressionType.TypeName} to {type.TypeName}.", context.Start.Line, context.Start.Column));
            }

            if (!_vtable.AddEntry(context.ID().GetText(), type))
            {
                _errors.Add(new CompilerError($"Variable '{context.ID().GetText()}' already exists.", context.Start.Line, context.Start.Column));
                return LookupVariable(context.ID().GetText(), context.Start.Line, context.Start.Column);
            }

            return type;
        }

        public override SctType VisitIDExpression([NotNull] SctParser.IDExpressionContext context)
        {
            return LookupVariable(context.ID().GetText(), context.Start.Line, context.Start.Column);
        }

        public override SctType VisitType([NotNull] SctParser.TypeContext context)
        {
            var type = TypeTable.GetType(context.GetText()) ?? throw new InvalidTypeException($"Type '{context.GetText()}' does not exist");
            return type;
        }

        public override SctType VisitClass_def([NotNull] SctParser.Class_defContext context)
        {
            // can never be null, as SctTableVisitor created the class
            _currentClass = _ctable.GetClassContent(context.ID().GetText())!;
            _vtable.EnterScope();
            _ = base.VisitClass_def(context);
            _currentClass = _ctable.GlobalClass;
            _vtable.ExitScope();
            return TypeTable.Ok;
        }


        public override SctType VisitEnter([NotNull] SctParser.EnterContext context)
        {
            var stateName = context.ID().GetText();
            // supress error if we are in the global class (return checker throws error instead)
            if (!_currentClass.HasState(stateName) && !(_currentClass == _ctable.GlobalClass))
            {
                _errors.Add(new CompilerError($"State '{stateName}' does not exist in class '{_currentClass.Name}'.", context.Start.Line, context.Start.Column));
            }
            return base.VisitEnter(context);
        }

        public override SctType VisitState_decorator([NotNull] SctParser.State_decoratorContext context)
        {
            var decoratorName = context.ID().GetText();
            if (!_currentClass.HasDecorator(decoratorName))
            {
                _errors.Add(new CompilerError($"Decorator '{decoratorName}' does not exist in class '{_currentClass.Name}'.", context.Start.Line, context.Start.Column));
            }
            return TypeTable.Ok;
        }

        public override SctType VisitFunction([NotNull] SctParser.FunctionContext context)
        {
            var functionName = context.ID().GetText();
            _currentFunctionType = GetFunctionType(functionName, context.Start.Line, context.Start.Column);

            // this only happens when multiple functions exist with the same name
            // we already log this in the SctTableVisitor (when we build the CTable)
            // but need this, to be able to continue typechecking
            if (_currentFunctionType.ReturnType != context.type().Accept(this))
            {
                return TypeTable.Void;
            }
            _vtable.EnterScope();
            _ = context.args_def().Accept(this);
            _ = context.type().Accept(this);
            _ = context.statement_list()?.Accept(this); // function may not have a body
            _vtable.ExitScope();
            return TypeTable.Ok;
        }

        public override SctType VisitArgs_def([NotNull] SctParser.Args_defContext context)
        {
            foreach (var (id, type) in context.ID().Zip(context.type()))
            {
                _ = _vtable.AddEntry(id.GetText(), type.Accept(this));
            }
            return TypeTable.Ok;
        }

        public override SctType VisitCallExpression([NotNull] SctParser.CallExpressionContext context)
        {
            var functionName = context.ID().GetText();
            var functionType = GetFunctionType(functionName, context.Start.Line, context.Start.Column);
            var functionParamTypes = functionType.ParameterTypes;
            var argumentTypes = context.args_call().expression().Select(expression => expression.Accept(this)).ToList();
            if (functionParamTypes.Count == argumentTypes.Count)
            {
                foreach (var (functionParamType, argumentType) in functionParamTypes.Zip(argumentTypes))
                {
                    if (TypeTable.GetCompatibleType(functionParamType, argumentType) is null)
                    {
                        _errors.Add(new CompilerError($"Cannot convert {argumentType.TypeName} to {functionParamType.TypeName} in call to function '{functionName}'.", context.Start.Line, context.Start.Column));
                    }
                }
            }
            else
            {
                _errors.Add(new CompilerError($"Function '{functionName}' expected {functionParamTypes.Count} arguments, but {argumentTypes.Count} were provided.", context.Start.Line, context.Start.Column));
            }
            return functionType.ReturnType;
        }

        public override SctType VisitUnaryMinusExpression([NotNull] SctParser.UnaryMinusExpressionContext context)
        {
            var expressionType = context.expression().Accept(this);
            if (!TypeTable.TypeIsNumeric(expressionType))
            {
                _errors.Add(new CompilerError("Unary minus expression must have numeric type.", context.Start.Line, context.Start.Column));
            }
            return expressionType;
        }

        public override SctType VisitReturn([NotNull] SctParser.ReturnContext context)
        {
            var returnType = _currentFunctionType.ReturnType;

            if (context.expression() is null)
            {
                if (returnType != TypeTable.Void)
                {
                    _errors.Add(new CompilerError($"Return type does not match the function's returned type, expected expression of type {returnType.TypeName}, got no expression.", context.Start.Line, context.Start.Column));
                }
                return TypeTable.Void;
            }

            var expressionType = context.expression().Accept(this);
            if (TypeTable.GetCompatibleType(returnType, expressionType) is null)
            {
                _errors.Add(new CompilerError($"Return type does not match the function's returned type, expected expression of type {returnType.TypeName}, got {expressionType.TypeName}.", context.Start.Line, context.Start.Column));
                expressionType = returnType;
            }
            return expressionType;
        }

        public override SctType VisitBinaryExpression([NotNull] SctParser.BinaryExpressionContext context)
        {
            var leftType = context.expression(0).Accept(this);
            var rightType = context.expression(1).Accept(this);
            if (!TypeTable.TypeIsNumeric(leftType) || !TypeTable.TypeIsNumeric(rightType))
            {
                _errors.Add(new CompilerError("Binary expression must have numeric types.", context.Start.Line, context.Start.Column));
                leftType = TypeTable.Int;
                rightType = TypeTable.Int;
            }

            return (leftType == rightType) ? leftType : TypeTable.Float; // If we have two different types (int and float), we return float.
        }

        public override SctType VisitLiteral([NotNull] SctParser.LiteralContext context)
        {
            return context switch
            {
                { } when context.INT() is not null => TypeTable.Int,
                { } when context.FLOAT() is not null => TypeTable.Float,
                _ => throw new InvalidTypeException("Literal type not recognized, must be int or float.")
            };
        }

        public override SctType VisitAssignment([NotNull] SctParser.AssignmentContext context)
        {
            var variableType = LookupVariable(context.ID().GetText(), context.Start.Line, context.Start.Column);
            var expressionType = context.expression().Accept(this);

            if (TypeTable.GetCompatibleType(variableType, expressionType) is null)
            {
                _errors.Add(new CompilerError($"Cannot assign {expressionType.TypeName} to {variableType.TypeName}.", context.Start.Line, context.Start.Column));
            }

            return variableType;
        }

        public override SctType VisitBooleanExpression([NotNull] SctParser.BooleanExpressionContext context)
        {
            var leftType = context.expression(0).Accept(this);
            var rightType = context.expression(1).Accept(this);
            if (// Types are not numeric
                (!TypeTable.TypeIsNumeric(leftType) || !TypeTable.TypeIsNumeric(rightType))
                // Types are not predicates or operators do not allow it
                && !(leftType == rightType && leftType == TypeTable.Predicate
                    && context.op.Type is SctLexer.EQ or SctLexer.NEQ)
                )
            {
                _errors.Add(new CompilerError("Boolean expression must be numeric types or predicate comparisons. For predicates, only (in)equality is allowed.", context.Start.Line, context.Start.Column));
            }
            return TypeTable.Int;
        }

        public override SctType VisitTypecastExpression([NotNull] SctParser.TypecastExpressionContext context)
        {
            var targetType = context.type().Accept(this);
            var expressionType = context.expression().Accept(this);
            if (!TypeTable.IsTypeCastable(expressionType, targetType))
            {
                _errors.Add(new CompilerError($"Cannot typecast from {expressionType.TypeName} to {targetType.TypeName}.", context.Start.Line, context.Start.Column));
            }
            return targetType;
        }

        public override SctType VisitAgent_predicate([NotNull] SctParser.Agent_predicateContext context)
        {
            var className = context.ID(0).GetText();
            var targetClass = _ctable.GetClassContent(className);

            if (targetClass is null)
            {
                _errors.Add(new CompilerError($"Class '{className}' does not exist.", context.Start.Line, context.Start.Column));
                return TypeTable.Predicate;
            }

            if (context.QUESTION() is null && !targetClass.HasState(context.ID(1).GetText()))
            {
                _errors.Add(new CompilerError($"State '{context.ID(1).GetText()}' does not exist in class '{className}'.", context.Start.Line, context.Start.Column));
            }

            var targetAgentFields = targetClass.Fields;
            var classArgumentIds = context.args_agent()?.ID() ?? [];
            var classArgumentExpressions = context.args_agent()?.expression() ?? [];


            var seenFields = new HashSet<string>();

            foreach (var (id, expression) in classArgumentIds.Zip(classArgumentExpressions))
            {
                if (!targetAgentFields.ContainsKey(id.GetText()))
                {
                    _errors.Add(new CompilerError($"Field '{id.GetText()}' does not exist in class '{className}'.", context.Start.Line, context.Start.Column));
                    continue;
                }

                if (!seenFields.Add(id.GetText()))
                {
                    _errors.Add(new CompilerError($"Duplicate argument '{id.GetText()}' in predicate.", context.Start.Line, context.Start.Column));
                    continue;
                }

                var targetClassFieldType = targetAgentFields[id.GetText()];
                var classFieldType = expression.Accept(this);
                if (TypeTable.GetCompatibleType(targetClassFieldType, classFieldType) is null)
                {
                    _errors.Add(new CompilerError($"Cannot convert {classFieldType.TypeName} to {targetClassFieldType.TypeName} in predicate.", context.Start.Line, context.Start.Column));
                }
            }

            return TypeTable.Predicate;
        }

        public override SctType VisitAgent_create([NotNull] SctParser.Agent_createContext context)
        {
            var className = context.ID(0).GetText();
            var stateName = context.ID(1).GetText();
            var targetClass = _ctable.GetClassContent(className);

            if (targetClass is null)
            {
                _errors.Add(new CompilerError($"Class '{className}' does not exist", context.Start.Line, context.Start.Column));
                return TypeTable.Ok;
            }

            if (!targetClass.HasState(stateName))
            {
                _errors.Add(new CompilerError($"State '{stateName}' does not exist in class '{className}'", context.Start.Line, context.Start.Column));
                return TypeTable.Ok;
            }

            var targetClassFields = targetClass.Fields;
            var classArgumentIds = context.args_agent()?.ID() ?? [];
            var classArgumentExpressions = context.args_agent()?.expression() ?? [];


            var seenFields = new HashSet<string>();

            foreach (var (id, expression) in classArgumentIds.Zip(classArgumentExpressions))
            {
                if (!targetClassFields.ContainsKey(id.GetText()))
                {
                    _errors.Add(new CompilerError($"Field '{id.GetText()}' does not exist in class '{className}'.", context.Start.Line, context.Start.Column));
                    continue;
                }

                if (!seenFields.Add(id.GetText()))
                {
                    _errors.Add(new CompilerError($"Duplicate argument '{id.GetText()}' in create statement.", context.Start.Line, context.Start.Column));
                    continue;
                }

                var targetClassFieldType = targetClassFields[id.GetText()];
                var classFieldType = expression.Accept(this);
                if (TypeTable.GetCompatibleType(targetClassFieldType, classFieldType) is null)
                {
                    _errors.Add(new CompilerError($"Cannot convert {classFieldType.TypeName} to {targetClassFieldType.TypeName}.", context.Start.Line, context.Start.Column));
                }
            }

            foreach (var field in targetClassFields)
            {
                if (!seenFields.Contains(field.Key))
                {
                    _errors.Add(new CompilerError($"Missing argument '{field.Key}' in create statement.", context.Start.Line, context.Start.Column));
                }
            }

            return TypeTable.Ok;
        }

        public override SctType VisitStatement_list([NotNull] SctParser.Statement_listContext context)
        {
            _vtable.EnterScope();
            _ = base.VisitStatement_list(context);
            _vtable.ExitScope();
            return TypeTable.Ok;
        }

        public override SctType VisitIf([NotNull] SctParser.IfContext context)
        {
            _ = CheckBooleanExpression(context.expression());
            _ = context.statement_list().Accept(this);
            if (context.elseif() is not null)
            {
                _ = context.elseif().Accept(this);
            }
            else if (context.@else() is not null)
            {
                _ = context.@else().Accept(this);
            }
            return TypeTable.Ok;
        }

        public override SctType VisitElseif([NotNull] SctParser.ElseifContext context)
        {
            _ = CheckBooleanExpression(context.expression());
            _ = context.statement_list().Accept(this);
            if (context.elseif() is not null)
            {
                _ = context.elseif().Accept(this);
            }
            else if (context.@else() is not null)
            {
                _ = context.@else().Accept(this);
            }
            return TypeTable.Ok;
        }

        public override SctType VisitWhile([NotNull] SctParser.WhileContext context)
        {
            _ = CheckBooleanExpression(context.expression());
            _ = context.statement_list().Accept(this);
            return TypeTable.Ok;
        }

        private bool CheckBooleanExpression(SctParser.ExpressionContext context)
        {
            var expressionType = context.Accept(this);
            if (!TypeTable.TypeIsNumeric(expressionType))
            {
                _errors.Add(new CompilerError("Boolean expression must be numeric.", context.Start.Line, context.Start.Column));
                return false;
            }
            return true;
        }
    }
}

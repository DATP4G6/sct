using Sct.Compiler.Syntax;

namespace Sct.Compiler.Typechecker
{
    public class SctTypeChecker : SctBaseSyntaxVisitor<SctType>
    {
        private readonly CTable _ctable;
        private readonly VTable _vtable = new();
        private ClassContent _currentClass;
        private FunctionType _currentFunctionType = new(SctType.Void, []);
        private readonly List<CompilerError> _errors = [];
        public IEnumerable<CompilerError> Errors => _errors;
        public SctTypeChecker(CTable cTable)
        {
            _ctable = cTable;
            _currentClass = _ctable.GlobalClass;
        }

        private void MakeChildrenAccept(SctSyntax node) => _ = node.Children.Select(c => c.Accept(this)).ToList();

        public override SctType Visit(SctProgramSyntax node)
        {
            MakeChildrenAccept(node);
            return SctType.Ok;
        }

        public override SctType Visit(SctFunctionSyntax node)
        {
            var functionName = node.Id;
            _currentFunctionType = GetFunctionType(functionName, node);

            // this only happens when multiple functions exist with the same name
            // we already log this in the SctTableVisitor (when we build the CTable)
            // but need this, to be able to continue typechecking
            if (_currentFunctionType.ReturnType != node.ReturnType.Accept(this))
            {
                return SctType.Void;
            }
            _vtable.EnterScope();

            foreach (var parameter in node.Parameters)
            {
                _ = parameter.Accept(this);
            }
            _ = node.ReturnType.Accept(this);
            _ = node.Block.Accept(this);
            _vtable.ExitScope();
            return SctType.Ok;
        }

        public override SctType Visit(SctIdExpressionSyntax node)
        {
            return LookupVariable(node.Id, node);
        }

        public override SctType Visit(SctTypeSyntax node)
        {
            return node.Type;
        }

        public override SctType Visit(SctClassSyntax node)
        {
            // can never be null, as SctTableVisitor created the class
            _currentClass = _ctable.GetClassContent(node.Id)!;
            _vtable.EnterScope();
            MakeChildrenAccept(node);
            _vtable.ExitScope();

            return SctType.Ok;
        }

        public override SctType Visit(SctEnterStatementSyntax node)
        {
            var stateName = node.Id;

            if (!_currentClass.HasState(stateName) && !(_currentClass == _ctable.GlobalClass))
            {
                _errors.Add(new CompilerError($"State '{stateName}' does not exist.", node.Context));
            }

            return SctType.Ok;
        }

        public override SctType Visit(SctStateSyntax node)
        {
            foreach (var decorator in node.Decorations)
            {
                if (!_currentClass.HasDecorator(decorator))
                {
                    _errors.Add(new CompilerError($"Decorator '{decorator}' does not exist.", node.Context));
                }
            }

            MakeChildrenAccept(node);
            return SctType.Ok;
        }

        public override SctType Visit(SctDecoratorSyntax node)
        {
            MakeChildrenAccept(node);
            return SctType.Ok;
        }

        public override SctType Visit(SctParameterSyntax node)
        {
            _ = _vtable.AddEntry(node.Id, node.Type.Accept(this));
            return SctType.Ok;
        }

        public override SctType Visit(SctCallExpressionSyntax node)
        {
            var functionType = GetFunctionType(node.Target, node);
            if (functionType.ParameterTypes.Count == node.Expressions.Count())
            {
                foreach (var (expression, parameterType) in node.Expressions.Zip(functionType.ParameterTypes))
                {
                    var expressionType = expression.Accept(this);
                    var compatibleType = GetCompatibleType(parameterType, expressionType);

                    if (compatibleType is null)
                    {
                        _errors.Add(new CompilerError($"Type mismatch in function call. Expected {parameterType.TypeName()}, or compatible type, but got {expressionType.TypeName()}", node.Context));
                    }
                }
            }
            else
            {
                _errors.Add(new CompilerError($"Function '{node.Target}' expects {functionType.ParameterTypes.Count} arguments, but got {node.Expressions.Count()}", node.Context));
            }

            return functionType.ReturnType;
        }

        public override SctType Visit(SctUnaryMinusExpressionSyntax node)
        {
            var expressionType = node.Expression.Accept(this);

            if (!TypeIsNumeric(expressionType))
            {
                _errors.Add(new CompilerError("Unary minus expression must be numeric type", node.Context));
                expressionType = SctType.Int;
            }
            return expressionType;
        }

        public override SctType Visit(SctReturnStatementSyntax node)
        {
            var returnType = _currentFunctionType.ReturnType;

            if (node.Expression is null)
            {
                if (returnType != SctType.Void)
                {
                    _errors.Add(new CompilerError($"Type mismatch in return statement. Expected {returnType.TypeName()} but got void", node.Context));
                }
                return SctType.Void;
            }

            var expressionType = node.Expression.Accept(this);
            if (GetCompatibleType(returnType, expressionType) is null)
            {
                _errors.Add(new CompilerError($"Type mismatch in return statement. Expected {returnType.TypeName()} but got {expressionType.TypeName()}", node.Context));
                expressionType = returnType;
            }
            return expressionType;
        }

        public override SctType Visit(SctBinaryExpressionSyntax node)
        {
            var leftType = node.Left.Accept(this);
            var rightType = node.Right.Accept(this);

            if (!TypeIsNumeric(leftType) || !TypeIsNumeric(rightType))
            {
                _errors.Add(new CompilerError("Binary expression must be numeric types", node.Context));
                leftType = SctType.Int;
                rightType = SctType.Int;
            }

            return (leftType == rightType) ? leftType : SctType.Float; // If we have two different types (int and float), we return float.
        }

        public override SctType Visit(SctAssignmentStatementSyntax node)
        {
            var targetType = LookupVariable(node.Id, node);
            var expressionType = node.Expression.Accept(this);

            if (GetCompatibleType(targetType, expressionType) is null)
            {
                _errors.Add(new CompilerError($"Type mismatch in assignment. Cannot assign {expressionType.TypeName()} to {targetType.TypeName()}", node.Context));
            }

            return targetType;
        }

        public override SctType Visit(SctBooleanExpressionSyntax node)
        {
            var leftType = node.Left.Accept(this);
            var rightType = node.Right.Accept(this);

            if (// Types are not numeric
                (!TypeIsNumeric(leftType) || !TypeIsNumeric(rightType))
                // Types are not predicates or operators do not allow it
                && !(leftType == rightType && leftType == SctType.Predicate
                    && node.Op is SctBooleanOperator.Eq or SctBooleanOperator.Neq)
                )
            {
                _errors.Add(new CompilerError("Boolean expression must be numeric types or predicate comparisons. For predicates, only (in)equality is allowed.", node.Context));
            }

            return SctType.Int;
        }

        public override SctType Visit(SctTypecastExpressionSyntax node)
        {
            var targetType = node.Type.Accept(this);
            var expressionType = node.Expression.Accept(this);

            if (!IsTypeCastable(expressionType, targetType))
            {
                _errors.Add(new CompilerError($"Type mismatch in typecast. Cannot cast {expressionType.TypeName()} to {targetType.TypeName()}", node.Context));
            }

            return targetType;
        }

        public override SctType Visit(SctPredicateExpressionSyntax node)
        {
            var target = _ctable.GetClassContent(node.ClassName);

            // Check if target class exists
            if (target is null)
            {
                _errors.Add(new CompilerError($"Species '{node.ClassName}' does not exist.", node.Context));
                return SctType.Predicate;
            }

            // Check if state exists in predicate and target class
            if (node.StateName is not null && !target.HasState(node.StateName))
            {
                _errors.Add(new CompilerError($"State '{node.StateName}' does not exist in species '{node.ClassName}'.", node.Context));
            }

            // Check if all fields exist in target class
            var seenFields = new HashSet<string>();

            foreach (var field in node.Fields)
            {
                if (!target.Fields.ContainsKey(field.Id))
                {
                    _errors.Add(new CompilerError($"Field '{field.Id}' does not exist in species '{node.ClassName}'.", field.Context));
                    continue;
                }

                if (!seenFields.Add(field.Id))
                {
                    _errors.Add(new CompilerError($"Field '{field.Id}' is already defined in predicate.", field.Context));
                    continue;
                }

                var targetClassFieldType = target.Fields[field.Id];
                var targetFieldExpressionType = field.Expression.Accept(this);

                if (GetCompatibleType(targetClassFieldType, targetFieldExpressionType) is null)
                {
                    _errors.Add(new CompilerError($"Type mismatch in predicate. Expected {targetClassFieldType.TypeName()}, or compatible type, but got {targetFieldExpressionType.TypeName()}", field.Context));
                }
            }

            return SctType.Predicate;
        }

        public override SctType Visit(SctIfStatementSyntax node)
        {
            if (node.Expression.Accept(this) != SctType.Int)
            {
                _errors.Add(new CompilerError($"Type mismatch in if statement. Condition must be boolean", node.Context));
            }

            if (node.Then != null)
            {
                _ = node.Then.Accept(this);
            }

            if (node.Else != null)
            {
                _ = node.Else.Accept(this);
            }

            return SctType.Ok;
        }

        public override SctType Visit(SctElseStatementSyntax node)
        {
            MakeChildrenAccept(node);
            return SctType.Ok;
        }

        public override SctType Visit(SctWhileStatementSyntax node)
        {
            if (node.Expression.Accept(this) != SctType.Int)
            {
                _errors.Add(new CompilerError($"Type mismatch in while statement. Condition must be numerical", node.Context));
            }

            _ = node.Block.Accept(this);
            return SctType.Ok;
        }


        public override SctType Visit(SctLiteralExpressionSyntax<long> node)
        {
            if (GetCompatibleType(SctType.Int, node.Type) is null)
            {
                _errors.Add(new CompilerError($"Type mismatch in literal expression. Expected {SctType.Int} but got {node.Type.TypeName()}", node.Context));
            }

            return SctType.Int;
        }

        public override SctType Visit(SctBreakStatementSyntax node)
        {
            return SctType.Ok;
        }

        public override SctType Visit(SctLiteralExpressionSyntax<double> node)
        {
            if (GetCompatibleType(SctType.Float, node.Type) is null)
            {
                _errors.Add(new CompilerError($"Type mismatch in literal expression. Expected {SctType.Float} but got {node.Type.TypeName()}", node.Context));
            }

            return SctType.Float;
        }

        public override SctType Visit(SctDeclarationStatementSyntax node)
        {
            var type = node.Type.Accept(this);
            if (type == SctType.Void)
            {
                _errors.Add(new CompilerError($"Variable {node.Id} cannot be of type {type.TypeName()}", node.Context));
                return SctType.Int;
            }

            if (node.Expression != null)
            {
                SctType expressionType = node.Expression.Accept(this);
                if (GetCompatibleType(type, expressionType) is null)
                {
                    _errors.Add(new CompilerError($"Type mismatch in declaration of variable {node.Id}. Expected {node.Type.Type.TypeName()} but got {expressionType.TypeName()}", node.Context));
                }
                else if (type == SctType.Void)
                {
                    _errors.Add(new CompilerError($"Variable {node.Id} cannot be of type {type.TypeName()}", node.Context));
                }
            }
            else
            {
                _errors.Add(new CompilerError($"Variable {node.Id} is not initialized", node.Context));
            }

            if (_vtable.AddEntry(node.Id, type) is false)
            {
                _errors.Add(new CompilerError($"Variable '{node.Id}' is already defined in this scope.", node.Context));
                return LookupVariable(node.Id, node);
            }

            return type;
        }

        public override SctType Visit(SctBlockStatementSyntax node)
        {
            _vtable.EnterScope();
            MakeChildrenAccept(node);
            _vtable.ExitScope();

            return SctType.Ok;
        }

        // This is the "create Agent::State(FieldID: Expression, ...)" syntax, not to be confused with the definition of an agent.
        public override SctType Visit(SctAgentExpressionSyntax node)
        {
            var target = _ctable.GetClassContent(node.ClassName);

            if (target is null)
            {
                _errors.Add(new CompilerError($"Species '{node.ClassName}' does not exist.", node.Context));
                return SctType.Ok;
            }

            if (!target.HasState(node.StateName))
            {
                _errors.Add(new CompilerError($"State '{node.StateName}' does not exist in species '{node.ClassName}'.", node.Context));
                return SctType.Ok;
            }

            var targetClassFields = target.Fields;
            var classArgumentIds = node.Fields.Select(field => field.Id);

            var seenFields = new HashSet<string>();

            foreach (var field in node.Fields)
            {

                if (!targetClassFields.ContainsKey(field.Id))
                {
                    _errors.Add(new CompilerError($"Field '{field.Id}' does not exist in species '{node.ClassName}'.", field.Context));
                    continue;
                }

                if (!seenFields.Add(field.Id))
                {
                    _errors.Add(new CompilerError($"Field '{field.Id}' is already defined in agent.", field.Context));
                    continue;
                }

                var targetClassFieldType = targetClassFields[field.Id];
                var targetFieldExpressionType = field.Expression.Accept(this);

                if (GetCompatibleType(targetClassFieldType, targetFieldExpressionType) is null)
                {
                    _errors.Add(new CompilerError($"Type mismatch in agent expression. Expected {targetClassFieldType.TypeName()}, or compatible type, but got {targetFieldExpressionType.TypeName()}", field.Context));
                }
            }

            foreach (var field in targetClassFields)
            {
                if (!seenFields.Contains(field.Key))
                {
                    _errors.Add(new CompilerError($"Missing argument '{field.Key}' in create statement.", node.Context));
                }
            }

            return SctType.Ok;
        }

        private FunctionType GetFunctionType(string functionName, SctSyntax node)
        {
            if (_currentClass.LookupFunctionType(functionName) is FunctionType functionType)
            {
                return functionType;
            }
            else if (_ctable.GlobalClass.LookupFunctionType(functionName) is FunctionType globalFunctionType)
            {
                return globalFunctionType;
            }
            _errors.Add(new CompilerError($"Function '{functionName}' does not exist.", node.Context));
            return new FunctionType(SctType.Void, []);
        }

        private static bool TypeIsNumeric(SctType type) => type is SctType.Int or SctType.Float;

        private static bool IsTypeCastable(SctType from, SctType to) => from == to || (TypeIsNumeric(from) && TypeIsNumeric(to));

        private static SctType? GetCompatibleType(SctType left, SctType right)
        {
            if (left == right)
            {
                return left;
            }
            else if (left == SctType.Float && right == SctType.Int)
            {
                return SctType.Float;
            }
            return null;
        }

        private SctType LookupVariable(string variableName, SctSyntax node)
        {
            SctType? variableType = _vtable.Lookup(variableName);

            if (variableType is null)
            {
                _errors.Add(new CompilerError($"Variable '{variableName}' does not exist.", node.Context));
                return SctType.Int;
            }
            return (SctType)variableType;
        }
    }
}

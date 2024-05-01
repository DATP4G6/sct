using Sct.Compiler.Syntax;

namespace Sct.Compiler.Typechecker
{
    public class SctAstTypeChecker : SctBaseSyntaxVisitor<Syntax.SctType>
    {
        private readonly CTable _ctable;
        private readonly VTable _vtable = new();
        private ClassContent _currentClass;
        private FunctionType _currentFunctionType = new(Syntax.SctType.Void, []);
        private readonly List<CompilerError> _errors = [];
        public IEnumerable<CompilerError> Errors => _errors;
        public SctAstTypeChecker(CTable cTable)
        {
            _ctable = cTable;
            _currentClass = _ctable.GlobalClass;
        }

        public override Syntax.SctType Visit(SctProgramSyntax node)
        {

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctFunctionSyntax node)
        {

            var functionName = node.Id;
            _currentFunctionType = GetFunctionType(functionName, node);

            // this only happens when multiple functions exist with the same name
            // we already log this in the SctTableVisitor (when we build the CTable)
            // but need this, to be able to continue typechecking
            if (_currentFunctionType.ReturnType != Visit(node.ReturnType))
            {
                return Syntax.SctType.Void;
            }
            _vtable.EnterScope();
            _ = node.Parameters.Select(Visit);
            _ = Visit(node.ReturnType);
            _ = Visit(node.Block);
            _vtable.ExitScope();
            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctParenthesisExpressionSyntax node)
        {
            return VisitChildren(node);
        }

        public override Syntax.SctType Visit(SctIdExpressionSyntax node)
        {

            return LookupVariable(node.Id, node);
        }

        public override Syntax.SctType Visit(SctTypeSyntax node)
        {

            return node.Type;
        }

        public override Syntax.SctType Visit(SctClassSyntax node)
        {
            // can never be null, as SctTableVisitor created the class
            _currentClass = _ctable.GetClassContent(node.Id)!;
            _vtable.EnterScope();
            _ = VisitChildren(node);
            _vtable.ExitScope();

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctEnterStatementSyntax node)
        {

            var stateName = node.Id;

            if (!_currentClass.HasState(stateName) && !(_currentClass == _ctable.GlobalClass))
            {
                _errors.Add(new CompilerError($"State '{stateName}' does not exist.", node.Context));
            }

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctDecoratorSyntax node)
        {

            if (!_currentClass.HasDecorator(node.Id))
            {
                _errors.Add(new CompilerError($"Decorator '{node.Id}' does not exist.", node.Context));

            }
            _ = VisitChildren(node);
            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctNamedArgumentSyntax node)
        {
            return VisitChildren(node);
        }

        public override Syntax.SctType Visit(SctParameterSyntax node)
        {
            _ = _vtable.AddEntry(node.Id, Visit(node.Type));
            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctCallExpressionSyntax node)
        {
            var functionType = GetFunctionType(node.Target, node);

            if (functionType.ParameterTypes.Count == node.Expressions.Count())
            {

                foreach (var (expression, parameterType) in node.Expressions.Zip(functionType.ParameterTypes))
                {
                    if (GetCompatibleType(Visit(expression), parameterType) is null)
                    {
                        _errors.Add(new CompilerError($"Type mismatch in function call. Expected {parameterType}, or compatible type, but got {expression}", node.Context));
                    }
                }
            }
            else
            {
                _errors.Add(new CompilerError($"Function '{node.Target}' expects {functionType.ParameterTypes.Count} arguments, but got {node.Expressions.Count()}", node.Context));
            }

            return functionType.ReturnType;
        }

        public override Syntax.SctType Visit(SctUnaryMinusExpressionSyntax node)
        {

            var expressionType = Visit(node.Expression);

            if (!TypeIsNumeric(expressionType))
            {
                _errors.Add(new CompilerError("Unary minus expression must be numeric type", node.Context));
                expressionType = Syntax.SctType.Int;
            }
            return expressionType;
        }

        public override Syntax.SctType Visit(SctReturnStatementSyntax node)
        {

            var returnType = _currentFunctionType.ReturnType;

            if (node.Expression is null)
            {

                if (returnType != Syntax.SctType.Void)
                {
                    _errors.Add(new CompilerError($"Type mismatch in return statement. Expected {returnType} but got void", node.Context));
                }
                return Syntax.SctType.Void;
            }

            var expressionType = Visit(node.Expression);
            if (GetCompatibleType(returnType, expressionType) is null)
            {
                _errors.Add(new CompilerError($"Type mismatch in return statement. Expected {returnType} but got {expressionType}", node.Context));
                expressionType = returnType;
            }
            return expressionType;
        }

        public override Syntax.SctType Visit(SctBinaryExpressionSyntax node)
        {

            var leftType = Visit(node.Left);
            var rightType = Visit(node.Right);

            if (!TypeIsNumeric(leftType) || !TypeIsNumeric(rightType))
            {
                _errors.Add(new CompilerError("Binary expression must be numeric types", node.Context));
                leftType = Syntax.SctType.Int;
                rightType = Syntax.SctType.Int;
            }

            return (leftType == rightType) ? leftType : Syntax.SctType.Float; // If we have two different types (int and float), we return float.
        }

        public override Syntax.SctType Visit(SctAssignmentStatementSyntax node)
        {

            var targetType = LookupVariable(node.Id, node);
            var expressionType = Visit(node.Expression);

            if (GetCompatibleType(targetType, expressionType) is null)
            {
                _errors.Add(new CompilerError($"Type mismatch in assignment. Cannot assign {expressionType} to {targetType}", node.Context));
            }

            return targetType;
        }

        public override Syntax.SctType Visit(SctBooleanExpressionSyntax node)
        {

            var leftType = Visit(node.Left);
            var rightType = Visit(node.Right);

            if (// Types are not numeric
                (!TypeIsNumeric(leftType) || !TypeIsNumeric(rightType))
                // Types are not predicates or operators do not allow it
                && !(leftType == rightType && leftType == Syntax.SctType.Predicate
                    && node.Op is Syntax.SctBooleanOperator.Eq or Syntax.SctBooleanOperator.Neq)
                )
            {
                _errors.Add(new CompilerError("Boolean expression must be numeric types or predicate comparisons. For predicates, only (in)equality is allowed.", node.Context));
            }

            return Syntax.SctType.Int;
        }

        public override Syntax.SctType Visit(SctTypecastExpressionSyntax node)
        {

            var targetType = Visit(node.Type);
            var expressionType = Visit(node.Expression);

            if (!IsTypeCastable(expressionType, targetType))
            {
                _errors.Add(new CompilerError($"Type mismatch in typecast. Cannot cast {expressionType} to {targetType}", node.Context));
            }

            return targetType;
        }

        public override Syntax.SctType Visit(SctPredicateExpressionSyntax node)
        {

            var target = _ctable.GetClassContent(node.ClassName);

            // Check if target class exists
            if (target is null)
            {
                _errors.Add(new CompilerError($"Class '{node.ClassName}' does not exist.", node.Context));
                return Syntax.SctType.Predicate;
            }

            // Check if state exists in predicate and target class
            if (node.StateName is not null && !target.HasState(node.StateName))
            {
                _errors.Add(new CompilerError($"State '{node.StateName}' does not exist in class '{node.ClassName}'.", node.Context));
            }

            // Check if all fields exist in target class

            var seenFields = new HashSet<string>();

            foreach (var field in node.Fields)
            {

                if (!target.Fields.ContainsKey(field.Id))
                {
                    _errors.Add(new CompilerError($"Field '{field.Id}' does not exist in class '{node.ClassName}'.", field.Context));
                    continue;
                }

                if (!seenFields.Add(field.Id))
                {
                    _errors.Add(new CompilerError($"Field '{field.Id}' is already defined in predicate.", field.Context));
                    continue;
                }

                var targetClassFieldType = target.Fields[field.Id];
                var targetFieldExpressionType = Visit(field.Expression);

                if (GetCompatibleType(targetClassFieldType, targetFieldExpressionType) is null)
                {
                    _errors.Add(new CompilerError($"Type mismatch in predicate. Expected {targetClassFieldType}, or compatible type, but got {targetFieldExpressionType}", field.Context));
                }
            }

            return Syntax.SctType.Predicate;
        }

        public override Syntax.SctType Visit(SctIfStatementSyntax node)
        {

            if (Visit(node.Expression) != Syntax.SctType.Int)
            {
                _errors.Add(new CompilerError($"Type mismatch in if statement. Condition must be boolean", node.Context));
            }

            if (node.Then != null)
            {
                _ = Visit(node.Then);
            }

            if (node.Else != null)
            {
                _ = Visit(node.Else);
            }

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctElseStatementSyntax node)
        {

            _ = VisitChildren(node);

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctWhileStatementSyntax node)
        {

            if (Visit(node.Expression) != Syntax.SctType.Int)
            {
                _errors.Add(new CompilerError($"Type mismatch in while statement. Condition must be numerical", node.Context));
            }

            _ = Visit(node.Block);

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctExpressionStatementSyntax node)
        {
            return VisitChildren(node);
        }

        public override Syntax.SctType Visit(SctLiteralExpressionSyntax<long> node)
        {

            if (node.Type != Syntax.SctType.Int)
            {
                _errors.Add(new CompilerError($"Type mismatch in literal expression. Expected {Syntax.SctType.Int} but got {node.Type}", node.Context));
            }

            return Syntax.SctType.Int;
        }

        public override Syntax.SctType Visit(SctLiteralExpressionSyntax<double> node)
        {

            if (node.Type != Syntax.SctType.Int)
            {
                _errors.Add(new CompilerError($"Type mismatch in literal expression. Expected {Syntax.SctType.Float} but got {node.Type}", node.Context));
            }

            return Syntax.SctType.Float;
        }

        public override Syntax.SctType Visit(SctDeclarationStatementSyntax node)
        {

            var type = Visit(node.Type);

            if (node.Expression != null)
            {
                Syntax.SctType expressionType = Visit(node.Expression);
                if (expressionType != type)
                {
                    _errors.Add(new CompilerError($"Type mismatch in declaration of variable {node.Id}. Expected {node.Type.Type} but got {expressionType}", node.Context));
                }
                else if (type == Syntax.SctType.Void)
                {
                    _errors.Add(new CompilerError($"Variable {node.Id} cannot be of type {type}", node.Context));
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

        public override Syntax.SctType Visit(SctBlockStatementSyntax node)
        {

            _vtable.EnterScope();
            _ = VisitChildren(node);
            _vtable.ExitScope();

            return Syntax.SctType.Ok;
        }

        // This is the "create Agent::State(FieldID: Expression, ...)" syntax, not to be confused with the definition of an agent.
        public override Syntax.SctType Visit(SctAgentExpressionSyntax node)
        {

            var target = _ctable.GetClassContent(node.ClassName);

            if (target is null)
            {
                _errors.Add(new CompilerError($"Class '{node.ClassName}' does not exist.", node.Context));
                return Syntax.SctType.Ok;
            }

            if (!target.HasState(node.StateName))
            {
                _errors.Add(new CompilerError($"State '{node.StateName}' does not exist in class '{node.ClassName}'.", node.Context));
                return Syntax.SctType.Ok;
            }

            var targetClassFields = target.Fields;
            var classArgumentIds = node.Fields.Select(field => field.Id);

            var seenFields = new HashSet<string>();

            foreach (var field in node.Fields)
            {

                if (!targetClassFields.ContainsKey(field.Id))
                {
                    _errors.Add(new CompilerError($"Field '{field.Id}' does not exist in class '{node.ClassName}'.", field.Context));
                    continue;
                }

                if (!seenFields.Add(field.Id))
                {
                    _errors.Add(new CompilerError($"Field '{field.Id}' is already defined in agent.", field.Context));
                    continue;
                }

                var targetClassFieldType = targetClassFields[field.Id];
                var targetFieldExpressionType = Visit(field.Expression);

                if (GetCompatibleType(targetClassFieldType, targetFieldExpressionType) is null)
                {
                    _errors.Add(new CompilerError($"Type mismatch in agent expression. Expected {targetClassFieldType}, or compatible type, but got {targetFieldExpressionType}", field.Context));
                }
            }

            foreach (var field in targetClassFields)
            {
                if (!seenFields.Contains(field.Key))
                {
                    _errors.Add(new CompilerError($"Missing argument '{field.Key}' in create statement.", node.Context));
                }
            }

            return Syntax.SctType.Ok;
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
            return new FunctionType(Syntax.SctType.Void, []);
        }
        public static bool TypeIsNumeric(Syntax.SctType type) => type is Syntax.SctType.Int or Syntax.SctType.Float;

        public static bool IsTypeCastable(Syntax.SctType from, Syntax.SctType to) => from == to || (TypeIsNumeric(from) && TypeIsNumeric(to));

        public static Syntax.SctType? GetCompatibleType(Syntax.SctType left, Syntax.SctType right)
        {
            if (left == right)
            {
                return left;
            }
            else if (left == Syntax.SctType.Float && right == Syntax.SctType.Int)
            {
                return Syntax.SctType.Float;
            }
            return null;
        }

        private Syntax.SctType LookupVariable(string variableName, SctSyntax node)
        {
            Syntax.SctType? variableType = _vtable.Lookup(variableName);

            if (variableType is null)
            {
                _errors.Add(new CompilerError($"Variable '{variableName}' does not exist.", node.Context));
            }
            return (Syntax.SctType)variableType!;
        }
    }
}

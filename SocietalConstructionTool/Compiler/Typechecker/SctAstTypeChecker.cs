using Sct.Compiler.Syntax;

namespace Sct.Compiler.Typechecker
{
    public class SctAstTypeChecker : SctBaseSyntaxVisitor<Syntax.SctType>
    {
        private readonly CTable _ctable;
        private readonly VTable _vtable = new();
        private SpeciesContent _currentSpecies;
        private FunctionType _currentFunctionType = new(Syntax.SctType.Void, []);
        private readonly List<CompilerError> _errors = [];
        public IEnumerable<CompilerError> Errors => _errors;
        public SctAstTypeChecker(CTable cTable)
        {
            _ctable = cTable;
            _currentSpecies = _ctable.GlobalContent;
        }

        private void MakeChildrenAccept(SctSyntax node)
        {
            foreach (var child in node.Children)
            {
                _ = child.Accept(this);
            }
        }

        public override Syntax.SctType Visit(SctProgramSyntax node)
        {
            MakeChildrenAccept(node);
            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctFunctionSyntax node)
        {
            var functionName = node.Id;
            _currentFunctionType = GetFunctionType(functionName, node);

            // this only happens when multiple functions exist with the same name
            // we already log this in the SctTableVisitor (when we build the CTable)
            // but need this, to be able to continue typechecking
            if (_currentFunctionType.ReturnType != node.ReturnType.Accept(this))
            {
                return Syntax.SctType.Void;
            }
            _vtable.EnterScope();

            foreach (var parameter in node.Parameters)
            {
                _ = parameter.Accept(this);
            }
            _ = node.ReturnType.Accept(this);
            _ = node.Block.Accept(this);
            _vtable.ExitScope();
            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctIdExpressionSyntax node)
        {
            return LookupVariable(node.Id, node);
        }

        public override Syntax.SctType Visit(SctTypeSyntax node)
        {
            return node.Type;
        }

        public override Syntax.SctType Visit(SctSpeciesSyntax node)
        {
            // can never be null, as SctTableVisitor created the species
            _currentSpecies = _ctable.GetSpeciesContent(node.Id)!;
            _vtable.EnterScope();
            MakeChildrenAccept(node);
            _vtable.ExitScope();

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctEnterStatementSyntax node)
        {
            var stateName = node.Id;

            if (!_currentSpecies.HasState(stateName) && !(_currentSpecies == _ctable.GlobalContent))
            {
                _errors.Add(new CompilerError($"State '{stateName}' does not exist.", node.Context));
            }

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctStateSyntax node)
        {
            for (var i = 0; i < node.Decorations.Count(); i++)
            {
                if (!_currentSpecies.HasDecorator(node.Decorations.ElementAt(i)))
                {
                    _errors.Add(new CompilerError($"Decorator '{node.Decorations.ElementAt(i)}' does not exist.", node.Context));
                }
            }

            MakeChildrenAccept(node);

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctDecoratorSyntax node)
        {
            if (!_currentSpecies.HasDecorator(node.Id))
            {
                _errors.Add(new CompilerError($"Decorator '{node.Id}' does not exist.", node.Context));

            }
            MakeChildrenAccept(node);
            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctParameterSyntax node)
        {
            _ = _vtable.AddEntry(node.Id, node.Type.Accept(this));
            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctCallExpressionSyntax node)
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

        public override Syntax.SctType Visit(SctUnaryMinusExpressionSyntax node)
        {
            var expressionType = node.Expression.Accept(this);

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
                    _errors.Add(new CompilerError($"Type mismatch in return statement. Expected {returnType.TypeName()} but got void", node.Context));
                }
                return Syntax.SctType.Void;
            }

            var expressionType = node.Expression.Accept(this);
            if (GetCompatibleType(returnType, expressionType) is null)
            {
                _errors.Add(new CompilerError($"Type mismatch in return statement. Expected {returnType.TypeName()} but got {expressionType.TypeName()}", node.Context));
                expressionType = returnType;
            }
            return expressionType;
        }

        public override Syntax.SctType Visit(SctBinaryExpressionSyntax node)
        {

            var leftType = node.Left.Accept(this);
            var rightType = node.Right.Accept(this);

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
            var expressionType = node.Expression.Accept(this);

            if (GetCompatibleType(targetType, expressionType) is null)
            {
                _errors.Add(new CompilerError($"Type mismatch in assignment. Cannot assign {expressionType.TypeName()} to {targetType.TypeName()}", node.Context));
            }


            return targetType;
        }

        public override Syntax.SctType Visit(SctBooleanExpressionSyntax node)
        {

            var leftType = node.Left.Accept(this);
            var rightType = node.Right.Accept(this);

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
            var targetType = node.Type.Accept(this);
            var expressionType = node.Expression.Accept(this);

            if (!IsTypeCastable(expressionType, targetType))
            {
                _errors.Add(new CompilerError($"Type mismatch in typecast. Cannot cast {expressionType.TypeName()} to {targetType.TypeName()}", node.Context));
            }

            return targetType;
        }

        public override Syntax.SctType Visit(SctPredicateExpressionSyntax node)
        {
            var target = _ctable.GetSpeciesContent(node.SpeciesName);

            // Check if target species exists
            if (target is null)
            {
                _errors.Add(new CompilerError($"Species '{node.SpeciesName}' does not exist.", node.Context));
                return Syntax.SctType.Predicate;
            }

            // Check if state exists in predicate and target species
            if (node.StateName is not null && !target.HasState(node.StateName))
            {
                _errors.Add(new CompilerError($"State '{node.StateName}' does not exist in species '{node.SpeciesName}'.", node.Context));
                return Syntax.SctType.Predicate;
            }

            // Check if all fields exist in target species

            var seenFields = new HashSet<string>();

            foreach (var field in node.Fields)
            {
                if (!target.Fields.ContainsKey(field.Id))
                {
                    _errors.Add(new CompilerError($"Field '{field.Id}' does not exist in species '{node.SpeciesName}'.", field.Context));
                    continue;
                }

                if (!seenFields.Add(field.Id))
                {
                    _errors.Add(new CompilerError($"Field '{field.Id}' is already defined in predicate.", field.Context));
                    continue;
                }

                var targetSpeciesFieldType = target.Fields[field.Id];
                var targetFieldExpressionType = field.Expression.Accept(this);

                if (GetCompatibleType(targetSpeciesFieldType, targetFieldExpressionType) is null)
                {
                    _errors.Add(new CompilerError($"Type mismatch in predicate. Expected {targetSpeciesFieldType.TypeName()}, or compatible type, but got {targetFieldExpressionType.TypeName()}", field.Context));
                }
            }

            return Syntax.SctType.Predicate;
        }

        public override Syntax.SctType Visit(SctIfStatementSyntax node)
        {
            if (node.Expression.Accept(this) != Syntax.SctType.Int)
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

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctElseStatementSyntax node)
        {
            MakeChildrenAccept(node);

            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctWhileStatementSyntax node)
        {
            if (node.Expression.Accept(this) != Syntax.SctType.Int)
            {
                _errors.Add(new CompilerError($"Type mismatch in while statement. Condition must be numerical", node.Context));
            }

            _ = node.Block.Accept(this);

            return Syntax.SctType.Ok;
        }


        public override Syntax.SctType Visit(SctLiteralExpressionSyntax<long> node)
        {
            if (GetCompatibleType(Syntax.SctType.Int, node.Type) is null)
            {
                _errors.Add(new CompilerError($"Type mismatch in literal expression. Expected {Syntax.SctType.Int} but got {node.Type.TypeName()}", node.Context));
            }

            return Syntax.SctType.Int;
        }

        public override Syntax.SctType Visit(SctBreakStatementSyntax node)
        {
            return Syntax.SctType.Ok;
        }

        public override Syntax.SctType Visit(SctLiteralExpressionSyntax<double> node)
        {
            if (GetCompatibleType(Syntax.SctType.Float, node.Type) is null)
            {
                _errors.Add(new CompilerError($"Type mismatch in literal expression. Expected {Syntax.SctType.Float} but got {node.Type.TypeName()}", node.Context));
            }

            return Syntax.SctType.Float;
        }

        public override Syntax.SctType Visit(SctDeclarationStatementSyntax node)
        {
            var type = node.Type.Accept(this);
            if (type == Syntax.SctType.Void)
            {
                _errors.Add(new CompilerError($"Variable {node.Id} cannot be of type {type.TypeName()}", node.Context));
                return Syntax.SctType.Int;
            }

            if (node.Expression != null)
            {
                Syntax.SctType expressionType = node.Expression.Accept(this);
                if (GetCompatibleType(type, expressionType) is null)
                {
                    _errors.Add(new CompilerError($"Type mismatch in declaration of variable {node.Id}. Expected {node.Type.Type.TypeName()} but got {expressionType.TypeName()}", node.Context));
                }
                else if (type == Syntax.SctType.Void)
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

        public override Syntax.SctType Visit(SctBlockStatementSyntax node)
        {

            _vtable.EnterScope();
            MakeChildrenAccept(node);
            _vtable.ExitScope();

            return Syntax.SctType.Ok;
        }

        // This is the "create Agent::State(FieldID: Expression, ...)" syntax, not to be confused with the definition of an agent.
        public override Syntax.SctType Visit(SctAgentExpressionSyntax node)
        {

            var target = _ctable.GetSpeciesContent(node.SpeciesName);

            if (target is null)
            {
                _errors.Add(new CompilerError($"Species '{node.SpeciesName}' does not exist.", node.Context));
                return Syntax.SctType.Ok;
            }

            if (!target.HasState(node.StateName))
            {
                _errors.Add(new CompilerError($"State '{node.StateName}' does not exist in species '{node.SpeciesName}'.", node.Context));
                return Syntax.SctType.Ok;
            }

            var targetSpeciesFields = target.Fields;
            var speciesArgumentIds = node.Fields.Select(field => field.Id);

            var seenFields = new HashSet<string>();

            foreach (var field in node.Fields)
            {

                if (!targetSpeciesFields.ContainsKey(field.Id))
                {
                    _errors.Add(new CompilerError($"Field '{field.Id}' does not exist in species '{node.SpeciesName}'.", field.Context));
                    continue;
                }

                if (!seenFields.Add(field.Id))
                {
                    _errors.Add(new CompilerError($"Field '{field.Id}' is already defined in agent.", field.Context));
                    continue;
                }

                var targetSpeciesFieldType = targetSpeciesFields[field.Id];
                var targetFieldExpressionType = field.Expression.Accept(this);

                if (GetCompatibleType(targetSpeciesFieldType, targetFieldExpressionType) is null)
                {
                    _errors.Add(new CompilerError($"Type mismatch in agent expression. Expected {targetSpeciesFieldType.TypeName()}, or compatible type, but got {targetFieldExpressionType.TypeName()}", field.Context));
                }
            }

            foreach (var field in targetSpeciesFields)
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
            if (_currentSpecies.LookupFunctionType(functionName) is FunctionType functionType)
            {
                return functionType;
            }
            else if (_ctable.GlobalContent.LookupFunctionType(functionName) is FunctionType globalFunctionType)
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
                return Syntax.SctType.Int;
            }
            return (Syntax.SctType)variableType!;
        }
    }
}

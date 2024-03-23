using Antlr4.Runtime.Misc;

namespace Sct.Compiler.Typechecking
{
    public class SctTypeChecker(SctFTable fTable, SctFTable sTable, SctFTable dTable, SctCTable cTable) : SctBaseListener
    {
        private readonly SctFTable _fTable = fTable;
        private readonly SctFTable _sTable = sTable;
        private readonly SctFTable _dTable = dTable;
        private readonly SctVTable _vTable = new();
        private readonly SctCTable _cTable = cTable;
        private readonly StackAdapter<SctType> _stack = new();
        // Return gets its own stack, as to not interfere with the main stack for i.e. if statement expressions
        private readonly StackAdapter<SctType> _returnStack = new();
        public List<string> Errors { get; private set; } = new();
        public override void EnterClass_def([NotNull] SctParser.Class_defContext context)
        {
            _fTable.LoadNextScope();
            _sTable.LoadNextScope();
            _dTable.LoadNextScope();
            _vTable.EnterScope();

            Console.WriteLine("\n\n\n\nThe new tables contain:");
            Console.WriteLine(_fTable);
            Console.WriteLine();
            Console.WriteLine(_sTable);
            Console.WriteLine();
            Console.WriteLine(_dTable);
            Console.WriteLine();
            Console.WriteLine(_cTable);
            Console.WriteLine();
        }
        public override void ExitClass_def([NotNull] SctParser.Class_defContext context)
        {
            _fTable.UnloadLastScope();
            _sTable.UnloadLastScope();
            _dTable.UnloadLastScope();
            _vTable.ExitScope();
        }
        public override void EnterFunction([NotNull] SctParser.FunctionContext context)
        {
            _vTable.EnterScope();
            // Pushhing marker is not strictly necessary. ExitFunction could pop entire stack instead
            // This however handles the future case of nested functions
            _returnStack.PushMarker();
        }
        public override void ExitFunction([NotNull] SctParser.FunctionContext context)
        {
            var expectedReturnType = TypeTable.GetType(context.type().GetText());
            var returnedTypes = _returnStack.PopUntilMarker();
            if (expectedReturnType is null)
            {
                Console.WriteLine($"Function {context.ID().GetText()} has unknown return type {context.type().GetText()}");
            }
            else if (expectedReturnType != TypeTable.Void)
            {
                foreach (var returnType in returnedTypes)
                {
                    if (!TypeTable.IsCompatible(returnType, expectedReturnType))
                    {
                        Console.WriteLine($"Function {context.ID().GetText()} has return type {returnType} but expected {context.type().GetText()}");
                    }
                }
            }
            _vTable.ExitScope();
        }
        public override void ExitArgs_def([NotNull] SctParser.Args_defContext context)
        {
            context.ID().Zip(
                context.type(),
                (id, type) => (id.GetText(), type.GetText())).ToList()
                    .ForEach(x => _vTable.Bind(x.Item1, TypeTable.GetType(x.Item2)));
        }
        public override void ExitExpressionStatement([NotNull] SctParser.ExpressionStatementContext context) => _stack.Pop();
        public override void ExitVariableDeclaration([NotNull] SctParser.VariableDeclarationContext context)
        {
            var name = context.ID().GetText();
            var type = context.type().GetText();
            var expressionType = _stack.Pop();
            if (!TypeTable.IsCompatible(expressionType, type))
            {
                Console.WriteLine($"Variable {name} has type {expressionType} but expected {type}");
            }
            if (_vTable.Lookup(name) != null)
            {
                Console.WriteLine($"Variable {name} already exists");
            }
            _vTable.Bind(name, TypeTable.GetType(type));
        }
        public override void ExitAssignment([NotNull] SctParser.AssignmentContext context)
        {
            var name = context.ID().GetText();
            var type = _vTable.Lookup(name);
            if (type == null)
            {
                Console.WriteLine($"Variable {name} doesn't exists");
            }
            var expressionType = _stack.Pop();
            if (!TypeTable.IsCompatible(expressionType, type!))
            {
                Console.WriteLine($"Variable {name} has type {expressionType} but expected {type}");
            }
        }
        public override void EnterIf([NotNull] SctParser.IfContext context) => _vTable.EnterScope();
        public override void ExitIf([NotNull] SctParser.IfContext context)
        {
            var type = _stack.Pop();
            if (!TypeTable.IsNumber(type))
            {
                Console.WriteLine($"If statement condition has type {type} but expected int or float");
            }
            _vTable.ExitScope();
        }
        public override void EnterElseif([NotNull] SctParser.ElseifContext context) => _vTable.EnterScope();
        public override void ExitElseif([NotNull] SctParser.ElseifContext context)
        {
            var type = _stack.Pop();
            if (!TypeTable.IsNumber(type))
            {
                Console.WriteLine($"Elseif statement condition has type {type} but expected int or float");
            }
            _vTable.ExitScope();
        }
        public override void EnterElse([NotNull] SctParser.ElseContext context) => _vTable.EnterScope();
        public override void ExitElse([NotNull] SctParser.ElseContext context) => _vTable.ExitScope();
        public override void EnterWhile([NotNull] SctParser.WhileContext context) => _vTable.EnterScope();
        public override void ExitWhile([NotNull] SctParser.WhileContext context)
        {
            var type = _stack.Pop();
            if (!TypeTable.IsNumber(type))
            {
                Console.WriteLine($"While statement condition has type {type} but expected int or float");
            }
        }
        public override void ExitEnter([NotNull] SctParser.EnterContext context)
        {
            if (_sTable.Lookup(context.ID().GetText()) == null)
            {
                Console.WriteLine($"State {context.ID().GetText()} doesn't exists");
            }
        }
        public override void ExitReturn([NotNull] SctParser.ReturnContext context)
        {
            if (context.expression() != null)
            {
                var type = _stack.Pop();
                _returnStack.Push(type);
            }
        }
        // TODO: Scope check enter, destroy, exit, break, continue, return
        public override void ExitLiteralExpression([NotNull] SctParser.LiteralExpressionContext context)
        {
            var text = context.LIT().GetText();
            if (double.TryParse(text, out var value))
            {
                _stack.Push(value % 1 == 0 ? TypeTable.Int : TypeTable.Float);
            }
            else
            {
                Console.WriteLine($"Literal {text} is not a number");
            }
        }
        public override void ExitIDExpression([NotNull] SctParser.IDExpressionContext context)
        {
            var name = context.ID().GetText();
            var type = _vTable.Lookup(name);
            if (type is null)
            {
                Console.WriteLine($"Variable {name} doesn't exists");
            }
            _stack.Push(type ?? TypeTable.Int);
        }
        // ParenthesisExpression does nothing to types
        public override void ExitParenthesisExpression([NotNull] SctParser.ParenthesisExpressionContext context) { }
        public override void ExitTypecastExpression([NotNull] SctParser.TypecastExpressionContext context)
        {
            var text = context.type().GetText();
            var type = TypeTable.GetType(text);
            var expressionType = _stack.Pop();
            if (type is null || type == TypeTable.Void || type == TypeTable.Predicate)
            {
                Console.WriteLine($"Typecast cannot have type {text}");
            }
            else if (!TypeTable.IsExplicitlyCompatible(expressionType, type!))
            {
                Console.WriteLine($"Typecast has type {expressionType} but expected {type}");
            }
            // Guess int if type doesn't exist
            _stack.Push(type ?? TypeTable.Int);
        }
        public override void EnterCallExpression([NotNull] SctParser.CallExpressionContext context)
        {
            _stack.PushMarker();
        }
        public override void ExitCallExpression([NotNull] SctParser.CallExpressionContext context)
        {
            var name = context.ID().GetText();
            var rawFunction = _fTable.Lookup(name);
            var parameterTypes = _stack.PopUntilMarker();
            if (rawFunction is null)
            {
                Console.WriteLine($"Function {name} doesn't exists");
                _stack.Push(TypeTable.Int);
                return;
            }
            var function = (SctFunction)rawFunction;
            if (parameterTypes.Length != function.ParameterTypes.Length)
            {
                Console.WriteLine($"Function {name} has {function.ParameterTypes.Length} parameters but got {parameterTypes.Length}");
                _stack.Push(function.ReturnType);
                return;
            }
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if (!TypeTable.IsCompatible(parameterTypes[i], function.ParameterTypes[i]))
                {
                    Console.WriteLine($"Function {name} has parameter {i} with type {parameterTypes[i]} but expected {function.ParameterTypes[i]}");
                }
            }
            _stack.Push(function.ReturnType);
        }
        public override void EnterArgs_call([NotNull] SctParser.Args_callContext context)
        {
            // TODO: FOR SOME REASON ENTERCALL IS NEVER CALLED BEFORE THIS, SO WE PUSH THE MARKER HERE INSTEAD
            // TODO: MEGA CURSED
            _stack.PushMarker();
        }
        public override void ExitUnaryMinusExpression([NotNull] SctParser.UnaryMinusExpressionContext context)
        {
            var type = _stack.Pop();
            if (!TypeTable.IsNumber(type))
            {
                Console.WriteLine($"Unary minus has type {type} but expected int or float");
            }
            _stack.Push(type ?? TypeTable.Int);
        }
        public override void ExitLogicalNotExpression([NotNull] SctParser.LogicalNotExpressionContext context)
        {
            var type = _stack.Pop();
            if (TypeTable.IsNumber(type))
            {
                Console.WriteLine($"Logical not has type {type} but expected int or float");
            }
            _stack.Push(TypeTable.Int);
        }
        public override void ExitBinaryExpression([NotNull] SctParser.BinaryExpressionContext context)
        {
            var right = _stack.Pop();
            var left = _stack.Pop();
            var op = context.op.Text;
            if (!TypeTable.IsNumber(left) || !TypeTable.IsNumber(right))
            {
                Console.WriteLine($"Binary operator {op} has types {left} and {right} but expected int or float");
                _stack.Push(TypeTable.Int);
            }
            else if (left == TypeTable.Int || right == TypeTable.Int)
            {
                _stack.Push(TypeTable.Int);
            }
            else
            {
                _stack.Push(TypeTable.Float);
            }
        }
        public override void ExitBooleanExpression([NotNull] SctParser.BooleanExpressionContext context)
        {
            var right = _stack.Pop();
            var left = _stack.Pop();
            var op = context.op.Text;
            if (!TypeTable.IsNumber(left) || !TypeTable.IsNumber(right))
            {
                Console.WriteLine($"Boolean operator {op} has types {left} and {right} but expected int or float");
            }
            _stack.Push(TypeTable.Int);
        }
        public override void EnterDecorator([NotNull] SctParser.DecoratorContext context) => _vTable.EnterScope();
        public override void ExitDecorator([NotNull] SctParser.DecoratorContext context) => _vTable.ExitScope();
        public override void EnterState([NotNull] SctParser.StateContext context) => _vTable.EnterScope();
        public override void ExitState([NotNull] SctParser.StateContext context) => _vTable.ExitScope();
        public override void ExitState_decorator([NotNull] SctParser.State_decoratorContext context)
        {
            if (_dTable.Lookup(context.ID().GetText()) == null)
            {
                Console.WriteLine($"Decorator {context.ID().GetText()} doesn't exists");
            }
        }
        public override void EnterArgs_agent([NotNull] SctParser.Args_agentContext context) => _stack.PushMarker();
        private Dictionary<string, SctType> ParseExitArgs_agent(SctParser.Args_agentContext context)
        {
            var parameterTypes = _stack.PopUntilMarker();
            var parameters = context.ID().Zip(
                parameterTypes,
                (id, type) => (id.GetText(), type)).ToDictionary(x => x.Item1, x => x.type);
            return parameters;
        }
        public override void ExitAgent_create([NotNull] SctParser.Agent_createContext context)
        {
            var className = context.ID()[0].GetText();
            var stateName = context.ID()[1].GetText();
            var parameters = ParseExitArgs_agent(context.args_agent());

            // Check class
            if (!_cTable.ContainsClass(className))
            {
                Console.WriteLine($"Class {className} doesn't exists");
            }
            // Check state
            else if (!_cTable.LookupStates(className).Contains(stateName))
            {
                Console.WriteLine($"State {stateName} doesn't exists on class {className}");
            }
            // Ensure paramters are correct
            else
            {
                var classParameters = _cTable.LookupFields(className);
                foreach ((var name, var type) in parameters)
                {
                    if (!classParameters.ContainsKey(name))
                    {
                        Console.WriteLine($"Class {className} doesn't have parameter {name}");
                        continue;
                    }
                    if (!TypeTable.IsCompatible(type, classParameters[name]))
                    {
                        Console.WriteLine($"Class {className} got parameter {name} with type {type} but expected {classParameters[name]}");
                    }
                }
                foreach ((var name, var _) in classParameters)
                {
                    if (!parameters.ContainsKey(name))
                    {
                        Console.WriteLine($"Class {className} expected parameter {name} but didn't get it");
                    }
                }
            }
            // Push return type
            // _stack.Push(TypeTable.Predicate);
        }
        public override void ExitAgent_predicate([NotNull] SctParser.Agent_predicateContext context)
        {
            var className = context.ID()[0].GetText();
            var stateName = context.QUESTION()?.GetText() ?? context.ID()[1].GetText();
            var parameters = ParseExitArgs_agent(context.args_agent());

            // Check class
            if (!_cTable.ContainsClass(className))
            {
                Console.WriteLine($"Class {className} doesn't exists");
            }
            // Check state
            else if (stateName != context.QUESTION()?.GetText() && !_cTable.LookupStates(className).Contains(stateName))
            {
                Console.WriteLine($"State {stateName} doesn't exists on class {className}");
                return;
            }
            // Ensure paramters are correct
            else
            {
                var classParameters = _cTable.LookupFields(className);
                foreach ((var name, var type) in parameters)
                {
                    if (!classParameters.ContainsKey(name))
                    {
                        Console.WriteLine($"Class {className} doesn't have parameter {name}");
                        continue;
                    }
                    if (!TypeTable.IsCompatible(type, classParameters[name]))
                    {
                        Console.WriteLine($"Class {className} got parameter {name} with type {type} but expected {classParameters[name]}");
                    }
                }
            }

            // Push return type
            _stack.Push(TypeTable.Predicate);
        }
    }
}

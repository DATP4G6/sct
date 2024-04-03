using System.Globalization;

using Antlr4.Runtime.Misc;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Extensions;
using Sct.Runtime;

namespace Sct.Compiler
{
    public class SctTranslator : SctBaseListener
    {
        private static readonly SyntaxToken ContextIdentifier = SyntaxFactory.Identifier("ctx");
        private static readonly IdentifierNameSyntax ContextIdentifierName = SyntaxFactory.IdentifierName("ctx");
        // boolean values are either 0 or 1
        private static readonly LiteralExpressionSyntax SctTrue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1));
        private static readonly LiteralExpressionSyntax SctFalse = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));

        private const string NAME_MANGLE_PREFIX = "__sct_";

        public NamespaceDeclarationSyntax? Root { get; private set; }
        private readonly StackAdapter<CSharpSyntaxNode> _stack = new();
        private readonly TypeTable _typeTable = new();

        // These two could likely have been removed, had we decorated an AST first
        private bool _isInAgent;
        private List<string> _stateNames = new();

        public override void ExitStart(SctParser.StartContext context)
        {
            var members = _stack.ToArray<MemberDeclarationSyntax>();

            var @class = SyntaxFactory
            .ClassDeclaration("GlobalClass")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(members)
            .AddMembers(MakeMainMethod());

            var @using = SyntaxFactory.UsingDirective(
                SyntaxFactory.IdentifierName(typeof(BaseAgent).Namespace ?? "")
            );

            var @namespace = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("MyNamespace"))
            .AddMembers(@class)
            .AddUsings(@using);

            Root = @namespace;
        }

        public override void ExitClass_def([NotNull] SctParser.Class_defContext context)
        {
            _isInAgent = false;
            // Pop functions, decorators, and states, but throw away parameter list, as it is not relevant post-type checking
            // All constructors are equal, so we can just create a custom one
            var members = _stack.PopUntil<ParameterListSyntax, MemberDeclarationSyntax>(out var fields);
            var className = MangleStringName(context.ID().GetText());

            var @class = SyntaxFactory.ClassDeclaration(className)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(nameof(BaseAgent))))
            .AddMembers(CreateClassFields(fields))
            .AddMembers(CreateConstructor(className))
            .AddMembers(members)
            .AddMembers(CreateCloneMethod(className))
            .AddMembers(CreateUpdateMethod());

            _stack.Push(@class);
        }

        private MethodDeclarationSyntax CreateUpdateMethod()
        {

            var body = SyntaxFactory.Block().AddStatements(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.SwitchExpression(
                        SyntaxFactory.IdentifierName(nameof(BaseAgent.State)),
                        // Add cases for each state in stateNames, calling the corresponding method of the same name
                        SyntaxFactory.SeparatedList(
                            _stateNames.Select(stateName => SyntaxFactory.SwitchExpressionArm(
                                SyntaxFactory.ConstantPattern(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(stateName)
                                    )
                                ),
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.IdentifierName(stateName),
                                    WithContextArgument([])
                                )
                            ))
                        )
                    )
                )
            );

            _stateNames = new();

            var updateMethod = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                nameof(BaseAgent.Update)
            )
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
            .WithParameterList(WithContextParameter([]))
            .WithBody(body);

            return updateMethod;
        }

        private static MethodDeclarationSyntax CreateCloneMethod(string className)
        {
            // 'new <className>(State, Fields)'
            var dict = SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.ParseTypeName(className))
            .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    // with base arguments
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(nameof(BaseAgent.State))),
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(nameof(BaseAgent.Fields)))
                    })
                )
            );

            // Create clone method
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(typeof(BaseAgent).Name),
                nameof(BaseAgent.Clone)
            )
            // public override of BaseAgent's method
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
            .WithBody(SyntaxFactory.Block(
                // return the dictionary defined above
                SyntaxFactory.ReturnStatement(dict)
            ));
        }

        private static ConstructorDeclarationSyntax CreateConstructor(string className)
        {
            var stateIdentifier = SyntaxFactory.Identifier(nameof(BaseAgent.State).ToLower(CultureInfo.InvariantCulture));
            var fieldsIdentifier = SyntaxFactory.Identifier(nameof(BaseAgent.Fields).ToLower(CultureInfo.InvariantCulture));
            // base parameters for constructor is string 'state' and IDictionary<string, dynamic> 'fields'
            var parameters = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(new[]
                {
                    SyntaxFactory.Parameter(stateIdentifier)
                        .WithType(SyntaxFactory.ParseTypeName(typeof(string).Name)),
                    SyntaxFactory.Parameter(fieldsIdentifier)
                        .WithType(SyntaxFactory.ParseTypeName(typeof(IDictionary<string, dynamic>).GenericName()))
                })
            );

            return SyntaxFactory.ConstructorDeclaration(className)
            .WithParameterList(parameters) // Add parameters
            .WithBody(SyntaxFactory.Block()) // Add empty body
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            // Add base constructor call
            .WithInitializer(SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                SyntaxFactory.ArgumentList(
                    // BaseAgent takes two parameters
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(stateIdentifier)),
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(fieldsIdentifier))
                    })
                )
            ));
        }
        public override void ExitExpressionStatement([NotNull] SctParser.ExpressionStatementContext context)
        {
            // convert expression to statement in eg. a void function call
            var expression = _stack.Pop<ExpressionSyntax>();
            _stack.Push(SyntaxFactory.ExpressionStatement(expression));
        }

        public override void ExitArgs_def([NotNull] SctParser.Args_defContext context)
        {
            // create list of parameters by zipping ID and type
            var @params = context.ID().Select(id => MangleName(id.GetText()))
            .Zip(context.type(), (id, type) =>
                SyntaxFactory.Parameter(id) // set name
                    .WithType(SyntaxFactory.ParseTypeName(type.GetText())) // set type
            );

            // add to parameter list
            _stack.Push(SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(@params)
            ));
        }

        public override void EnterArgs_call([NotNull] SctParser.Args_callContext context)
        {
            _stack.PushMarker();
        }

        public override void ExitArgs_call(SctParser.Args_callContext context)
        {
            var args = _stack.PopUntilMarker<ExpressionSyntax>();
            _stack.Push(SyntaxFactory.ArgumentList( // pass expressions as argument list
                SyntaxFactory.SeparatedList(args.Select(SyntaxFactory.Argument))
            ));
        }

        public override void EnterArgs_agent([NotNull] SctParser.Args_agentContext context)
        {
            _stack.PushMarker();
        }

        public override void ExitArgs_agent([NotNull] SctParser.Args_agentContext context)
        {
            var args = _stack.PopUntilMarker<ExpressionSyntax>();

            // create list of args
            var keyValuePairs = args
            .Select((arg, i) => (expr: arg, name: MangleStringName(context.ID(i).GetText())))
            .Select(arg =>
                    // all arguments are of type KeyValuePair<string, dynamic>
                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(typeof(KeyValuePair<string, dynamic>).GenericName()))
                    .WithArgumentList(SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                        // set key to ID
                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(arg.name))),
                            SyntaxFactory.Argument(arg.expr) // set value to expression from stack
                        })
                    ))
            );

            var arrayExpression = SyntaxFactory.ArrayCreationExpression(
                SyntaxFactory.ArrayType(SyntaxFactory.ParseTypeName(typeof(KeyValuePair<string, dynamic>).GenericName() + "[]")),
                SyntaxFactory.InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(
                        keyValuePairs
                    )
                )
            );

            var dictionary = SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.ParseTypeName(typeof(Dictionary<string, dynamic>).GenericName())
                )
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[] {
                            SyntaxFactory.Argument(
                                arrayExpression
                            )
                        })
                    )
                );

            _stack.Push(dictionary);
        }

        public override void ExitDecorator([NotNull] SctParser.DecoratorContext context)
        {
            var childBlock = _stack.Pop<BlockSyntax>();
            childBlock = childBlock.AddStatements(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)));

            var mangledName = MangleName(context.ID().GetText());

            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), // all decorators return bool
                mangledName
            )
            .WithParameterList(WithContextParameter([])) // all decorators take 0 arguments;
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
            .WithBody(childBlock);

            _stack.Push(method);
        }

        public override void ExitFunction([NotNull] SctParser.FunctionContext context)
        {
            var childBlock = _stack.Pop<BlockSyntax>();
            var @params = WithContextParameter(_stack.Pop<ParameterListSyntax>());
            var mangledName = MangleName(context.ID().GetText());

            var method = SyntaxFactory.MethodDeclaration(
                _typeTable.GetTypeNode(context.type().GetText()),
                mangledName
            )
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithParameterList(@params)
            .WithBody(childBlock);

            if (!_isInAgent) // all global functions are static
            {
                method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
            }

            _stack.Push(method);
        }

        public override void ExitState_decorator([NotNull] SctParser.State_decoratorContext context)
        {
            var mangledName = MangleName(context.ID().GetText());
            // push decorator to the stack as method call
            var state = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(mangledName),
                WithContextArgument([])
            );

            _stack.Push(state);
        }

        public override void ExitVariableDeclaration([NotNull] SctParser.VariableDeclarationContext context)
        {
            var expression = _stack.Pop<ExpressionSyntax>();

            var mangledName = MangleName(context.ID().GetText());

            var variable = SyntaxFactory.VariableDeclaration(
                _typeTable.GetTypeNode(context.type().GetText()) // set type
            )
            .AddVariables(
                SyntaxFactory.VariableDeclarator(mangledName) // set name
                    .WithInitializer(SyntaxFactory.EqualsValueClause(expression)) // equal to expression
            );

            // convert to statement
            var statement = SyntaxFactory.LocalDeclarationStatement(variable);
            _stack.Push(statement);
        }

        public override void ExitAssignment([NotNull] SctParser.AssignmentContext context)
        {
            var expression = _stack.Pop<ExpressionSyntax>();
            var mangledName = MangleName(context.ID().GetText());
            var assignment = SyntaxFactory.ExpressionStatement( // convert to expression
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(mangledName), // assign name
                    expression // to expression
                )
            );
            _stack.Push(assignment);
        }

        public override void ExitState([NotNull] SctParser.StateContext context)
        {
            var stateLogic = _stack.Pop<BlockSyntax>();
            stateLogic = stateLogic.AddStatements(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)));

            // create statement list of decorators
            var decorators = _stack.PopWhile<InvocationExpressionSyntax>();

            var ifs = decorators.Select(decor =>
            {
                var return_block = SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
                return SyntaxFactory.IfStatement(decor, return_block);
            });


            // create new body with decorators first, then state logic
            var body = SyntaxFactory.Block()
            .AddStatements(ifs.ToArray())
            .AddStatements(stateLogic.Statements.ToArray());

            var name = MangleName(context.ID().GetText());
            _stateNames.Add(name.Text);
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                MangleName(context.ID().GetText())
            )
            .WithParameterList(WithContextParameter([])) // state only takes context
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
            .WithBody(body);

            _stack.Push(method);
        }

        public override void EnterStatement_list([NotNull] SctParser.Statement_listContext context)
        {
            _stack.PushMarker();
        }

        public override void ExitStatement_list([NotNull] SctParser.Statement_listContext context)
        {
            var statements = _stack.PopUntilMarker<StatementSyntax>();
            var block = SyntaxFactory.Block(statements); // convert all statements to a single block
            _stack.Push(block);
        }

        public override void ExitWhile([NotNull] SctParser.WhileContext context)
        {
            var childBlock = _stack.Pop<BlockSyntax>();
            var expression = _stack.Pop<ExpressionSyntax>();
            var condition = ConvertToBooleanCondition(expression);

            var @while = SyntaxFactory.WhileStatement(
                condition,
                childBlock
            );
            _stack.Push(@while);
        }

        /// <summary>
        /// Converts an expression to a boolean condion by comparing it to 0
        /// </summary>
        /// <param name="expression">Expression to be compared</param>
        /// <returns>expression != 0</returns>
        private static BinaryExpressionSyntax ConvertToBooleanCondition(ExpressionSyntax expression)
        {
            return SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, expression, SctFalse);
        }

        // TODO: Split this rule up if we want literals that aren't just numeric
        public override void ExitLiteralExpression([NotNull] SctParser.LiteralExpressionContext context)
        {
            // prevent decimal point from being a comma (based on locale), because... C#, see CS1305
            var culture = CultureInfo.InvariantCulture;
            var text = context.LIT().GetText();
            var value = double.Parse(text, culture);
            var literal = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                    // Passing the text of the token to Literal ensures that we print whole floats (e.g. 2.0) correctly.
                    // Ints are also printed correctly this way, since we preserve the text of the token
                    // This might be a problem in the future if we want literals that don't look like C# literals
                    SyntaxFactory.Literal(text, value)
            );
            _stack.Push(literal);
        }

        public override void ExitIDExpression([NotNull] SctParser.IDExpressionContext context)
        {
            var id = MangleName(context.ID().GetText());
            var idExp = SyntaxFactory.IdentifierName(id);
            _stack.Push(idExp);
        }

        public override void ExitBinaryExpression([NotNull] SctParser.BinaryExpressionContext context)
        {
            var exp2 = _stack.Pop<ExpressionSyntax>();
            var exp1 = _stack.Pop<ExpressionSyntax>();

            // switch expression does not need to descructure the context,
            // so we determine it based on 'when'
            var @operator = context.op switch
            {
                { } op when op == context.PLUS()?.Symbol => SyntaxKind.AddExpression,
                { } op when op == context.MINUS()?.Symbol => SyntaxKind.SubtractExpression,
                { } op when op == context.MULT()?.Symbol => SyntaxKind.MultiplyExpression,
                { } op when op == context.DIV()?.Symbol => SyntaxKind.DivideExpression,
                { } op when op == context.MOD()?.Symbol => SyntaxKind.ModuloExpression,
                _ => SyntaxKind.None
            };

            var binaryExpression = SyntaxFactory.BinaryExpression(@operator, exp1, exp2);
            _stack.Push(binaryExpression);
        }

        public override void ExitBooleanExpression([NotNull] SctParser.BooleanExpressionContext context)
        {
            var exp2 = _stack.Pop<ExpressionSyntax>();
            var exp1 = _stack.Pop<ExpressionSyntax>();

            var @operator = context.op switch
            {
                { } op when op == context.GT()?.Symbol => SyntaxKind.GreaterThanExpression,
                { } op when op == context.LT()?.Symbol => SyntaxKind.LessThanExpression,
                { } op when op == context.GTE()?.Symbol => SyntaxKind.GreaterThanOrEqualExpression,
                { } op when op == context.LTE()?.Symbol => SyntaxKind.LessThanOrEqualExpression,
                { } op when op == context.EQ()?.Symbol => SyntaxKind.EqualsExpression,
                { } op when op == context.NEQ()?.Symbol => SyntaxKind.NotEqualsExpression,
                { } op when op == context.AND()?.Symbol => SyntaxKind.LogicalAndExpression,
                { } op when op == context.OR()?.Symbol => SyntaxKind.LogicalOrExpression,
                _ => SyntaxKind.None
            };

            // convert to conditional, as booleans do not exist in SCT
            var condition = SyntaxFactory.BinaryExpression(@operator, exp1, exp2);
            _stack.Push(SyntaxFactory.ConditionalExpression(condition, SctTrue, SctFalse));
        }

        public override void ExitParenthesisExpression([NotNull] SctParser.ParenthesisExpressionContext context)
        {
            var node = _stack.Pop<ExpressionSyntax>();
            _stack.Push(SyntaxFactory.ParenthesizedExpression(node));
        }

        public override void ExitTypecastExpression([NotNull] SctParser.TypecastExpressionContext context)
        {
            var node = _stack.Pop<ExpressionSyntax>();
            var type = _typeTable.GetTypeNode(context.type().GetText());
            _stack.Push(SyntaxFactory.CastExpression(type, node));
        }

        public override void ExitLogicalNotExpression([NotNull] SctParser.LogicalNotExpressionContext context)
        {
            var expression = _stack.Pop<ExpressionSyntax>();
            var @operator = SyntaxKind.NotEqualsExpression;
            // compare with 0, and convert to SctBoolean via conditional
            var condition = SyntaxFactory.BinaryExpression(@operator, expression, SctFalse);
            _stack.Push(SyntaxFactory.ConditionalExpression(condition, SctFalse, SctTrue));
        }


        public override void ExitUnaryMinusExpression([NotNull] SctParser.UnaryMinusExpressionContext context)
        {
            var expression = _stack.Pop<ExpressionSyntax>();
            _stack.Push(SyntaxFactory.PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression, expression));
        }

        public override void ExitCallExpression([NotNull] SctParser.CallExpressionContext context)
        {
            var args = WithContextArgument(_stack.Pop<ArgumentListSyntax>());
            var id = SyntaxFactory.IdentifierName(MangleName(context.ID().GetText()));
            var call = SyntaxFactory.InvocationExpression(id, args); // convert to method call
            _stack.Push(call);
        }

        public override void ExitAgent_create([NotNull] SctParser.Agent_createContext context)
        {
            var fields = _stack.Pop<ObjectCreationExpressionSyntax>();
            var state = MangleStringName(context.ID(1).GetText());
            var type = MangleStringName(context.ID(0).GetText());

            var agent = SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.ParseTypeName(type))
            .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(state)
                        )),
                        SyntaxFactory.Argument(fields)
                    })
                )
            );

            var createAgent = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(ContextIdentifier),
                            SyntaxFactory.IdentifierName(nameof(IRuntimeContext.AgentHandler))),
                        SyntaxFactory.IdentifierName(nameof(IAgentHandler.CreateAgent))
                        )
                    )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                                SyntaxFactory.Argument(agent)
                            })
                        )
                    )
            );

            _stack.Push(createAgent);
        }

        public override void ExitAgent_predicate([NotNull] SctParser.Agent_predicateContext context)
        {
            var fields = _stack.Pop<ObjectCreationExpressionSyntax>();
            // TODO: Extract literals like this into method. It gets very repetitive.
            var className = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(MangleStringName(context.ID(0).GetText()))
            ));
            var state = context.QUESTION() switch
            {
                null => SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(MangleStringName(context.ID(1).GetText()))
                    )),
                _ => SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                        SyntaxKind.NullLiteralExpression
                    )),
            };
            var predicate = SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.ParseTypeName(nameof(QueryPredicate))
            ).WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList([
                        className,
                        state,
                        SyntaxFactory.Argument(fields)
                    ])
                )
            );
            _stack.Push(predicate);
        }

        public override void ExitBreak([NotNull] SctParser.BreakContext context)
        {
            // TODO: check if we can break before doing it
            _stack.Push(SyntaxFactory.BreakStatement());
        }

        public override void ExitContinue([NotNull] SctParser.ContinueContext context)
        {
            // TODO: check if we can continue before doing it
            _stack.Push(SyntaxFactory.ContinueStatement());
        }

        public override void ExitReturn([NotNull] SctParser.ReturnContext context)
        {
            // TODO: check if we can return before doing it
            var @return = SyntaxFactory.ReturnStatement();
            var valueExists = _stack.TryPeek<ExpressionSyntax>(out var peekedValue);
            if (valueExists)
            {
                _ = _stack.Pop();
                @return = @return.WithExpression(peekedValue);
            }
            _stack.Push(@return);
        }

        public override void ExitIf([NotNull] SctParser.IfContext context)
        {
            var @if = IfHelper();
            _stack.Push(@if);
        }
        public override void ExitElseif([NotNull] SctParser.ElseifContext context)
        {
            // Else if is not its own construct. Wrap @if in an else clause
            var @if = IfHelper();
            var @else = SyntaxFactory.ElseClause(@if);
            _stack.Push(@else);
        }
        public override void ExitElse([NotNull] SctParser.ElseContext context)
        {
            var childBlock = _stack.Pop<BlockSyntax>();
            var @else = SyntaxFactory.ElseClause(childBlock);
            _stack.Push(@else);
        }

        private IfStatementSyntax IfHelper()
        {
            // Stack order is either <block> <exp> or <else> <block> <exp>
            var @else = _stack.Peek() is ElseClauseSyntax ? _stack.Pop<ElseClauseSyntax>() : null;
            var block = _stack.Pop<BlockSyntax>();
            var expression = _stack.Pop<ExpressionSyntax>();
            var condition = ConvertToBooleanCondition(expression);

            // Add else if it exists
            var @if = SyntaxFactory.IfStatement(condition, block);
            return @else == null ? @if : @if.WithElse(@else);
        }

        public override void ExitExit([NotNull] SctParser.ExitContext context)
        {
            // call ExitRuntime on context
            _stack.Push(SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(ContextIdentifier),
                        SyntaxFactory.IdentifierName(nameof(IRuntimeContext.ExitRuntime))
                    )
                )
            ));

            // return true any time we exit
            _stack.Push(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)));
        }

        public override void ExitEnter([NotNull] SctParser.EnterContext context)
        {
            var state = SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(MangleStringName(context.ID().GetText()))
            );
            var invocation = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName(BaseAgent.EnterMethodName),
                    WithContextArgument([SyntaxFactory.Argument(state)])
                )
            );
            var @return = SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));

            // invoke Enter of BaseAgent, and return to prevent further execution
            _stack.Push(invocation);
            _stack.Push(@return);
        }

        public override void ExitDestroy([NotNull] SctParser.DestroyContext context)
        {
            // return true to stop further execution
            _stack.Push(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)));
        }

        /// <summary>
        /// Returns a <see cref="ParameterListSyntax"/> with an <see cref="IRuntimeContext"/> <code>ctx</code> parameter added.
        /// </summary>
        private static ParameterListSyntax WithContextParameter(ParameterListSyntax p) => WithContextParameter(p.Parameters);

        /// <summary>
        /// Returns a <see cref="ParameterListSyntax"/> with an <see cref="IRuntimeContext"/> <code>ctx</code> parameter added.
        /// </summary>
        private static ParameterListSyntax WithContextParameter(IEnumerable<ParameterSyntax> p)
        {
            // create new parameter list
            return SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(
                    p.Prepend( // prepend context to the original parameters
                        SyntaxFactory.Parameter(ContextIdentifier)
                        .WithType(SyntaxFactory.ParseTypeName(nameof(IRuntimeContext)))
                    )
                )
            );
        }


        private static ArgumentListSyntax WithContextArgument(IEnumerable<ArgumentSyntax> a) => SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(a.Prepend(
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(ContextIdentifier))
                        ))
                );
        private static ArgumentListSyntax WithContextArgument(ArgumentListSyntax a) => WithContextArgument(a.Arguments);

        private static SyntaxToken MangleName(SyntaxToken n) => MangleName(n.Text);
        private static SyntaxToken MangleName(string n) => SyntaxFactory.Identifier(MangleStringName(n));
        private static string MangleStringName(string n) => NAME_MANGLE_PREFIX + n;

        private static MethodDeclarationSyntax MakeMainMethod()
        {
            var argsId = SyntaxFactory.IdentifierName("args");

            var runtime = SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName(nameof(Runtime))
                )
                .AddVariables(
                    SyntaxFactory.VariableDeclarator(
                        SyntaxFactory.Identifier("runtime")
                    )
                    .WithInitializer(
                        SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.ObjectCreationExpression(
                                SyntaxFactory.ParseTypeName(nameof(Runtime))
                            )
                            // need to add empty argument list to invoke constructor
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SeparatedList<ArgumentSyntax>()
                                )
                            )
                        )
                    )
                )
            );

            // TODO: Make this use `RuntimeContextFactory.CreateFromArgs`
            var ctx = SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName(nameof(RuntimeContext))
                )
                .AddVariables(
                    SyntaxFactory.VariableDeclarator(
                        ContextIdentifier
                    )
                    .WithInitializer(
                        SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ParseTypeName(nameof(RuntimeContextFactory)),
                                        SyntaxFactory.IdentifierName(nameof(RuntimeContextFactory.CreateFromArgs)
                                    )
                                )
                            )
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(argsId) })
                                )
                            )
                        )
                    )
                )
            );

            var setup = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("__sct_Setup"),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(ContextIdentifierName)
                        })
                    )
                )
            );

            var run = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("runtime"),
                        SyntaxFactory.IdentifierName("Run")
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(ContextIdentifierName)
                        })
                    )
                )
            );

            var body = SyntaxFactory.Block()
            .AddStatements(runtime, ctx, setup, run);

            // `string[] args` parameter
            var argsParameter = SyntaxFactory.Parameter(argsId.Identifier)
                .WithType(SyntaxFactory.ParseTypeName(typeof(string[]).Name));

            var mainMethod = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier("Main")
            )
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .AddParameterListParameters(argsParameter)
            .WithBody(body);

            return mainMethod;
        }

        /// <summary>
        /// Create getters and setters for all fields in a class
        /// </summary>
        /// <param name="fields">Fields in the class</param>
        /// <returns></returns>
        private static PropertyDeclarationSyntax[] CreateClassFields(ParameterListSyntax fields)
        {
            var fieldDeclarations = fields.Parameters.Select(p =>
            {
                var name = p.Identifier.Text;
                var type = p.Type ?? SyntaxFactory.ParseTypeName("dynamic"); // should never happen. TODO: Find a way to use typeof()-like naming for "dynamic"

                var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithExpressionBody(
                    SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.ElementAccessExpression(
                            SyntaxFactory.IdentifierName(nameof(BaseAgent.Fields)),
                            CreateBracketedArgumentList(name)) // get value from dictionary
                    )
                )
                // accessordeclaration does not insert this aparently
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithExpressionBody(
                    SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.ElementAccessExpression(
                                SyntaxFactory.IdentifierName(nameof(BaseAgent.Fields)),
                                CreateBracketedArgumentList(name) // get value from dictionary
                            ),
                            SyntaxFactory.IdentifierName("value") // set equal to value from setter
                        )
                    )
                )
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                // convert to public property declaration
                return SyntaxFactory.PropertyDeclaration(type, name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .AddAccessorListAccessors(getter, setter);
            });

            return fieldDeclarations.ToArray();
        }

        /// <summary>
        /// Creates a bracketed argument, accessing a dictionary with a key
        /// Helper method for the CreateClassFields method
        /// </summary>
        /// <param name="name">The key to find</param>
        /// <returns></returns>
        private static BracketedArgumentListSyntax CreateBracketedArgumentList(string name)
        {
            return SyntaxFactory.BracketedArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(name)
                        )
                    )
                )
            );
        }
    }
}

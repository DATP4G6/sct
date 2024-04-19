using System.Globalization;

using Antlr4.Runtime.Misc;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Extensions;
using Sct.Runtime;

namespace Sct.Compiler.Translator
{
    public class SctTranslator : SctBaseListener
    {
        public const string GeneratedNamespace = "SctGenerated";
        public const string GeneratedGlobalClass = "GlobalClass";
        public const string RunSimulationFunctionName = "RunSimulation";

        public static readonly SyntaxToken ContextIdentifier = SyntaxFactory.Identifier("ctx");

        // boolean values are either 0 or 1
        private static readonly LiteralExpressionSyntax SctTrue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1));
        private static readonly LiteralExpressionSyntax SctFalse = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0));

        public NamespaceDeclarationSyntax? Root { get; private set; }
        private readonly StackAdapter<CSharpSyntaxNode> _stack = new();

        // These two could likely have been removed, had we decorated an AST first
        private bool _isInAgent;
        private List<string> _stateNames = new();

        public override void ExitStart(SctParser.StartContext context)
        {
            var members = _stack.ToArray<MemberDeclarationSyntax>();

            var @class = SyntaxFactory
            .ClassDeclaration(GeneratedGlobalClass)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(members)
            .AddMembers(TranslatorUtils.MakeRunMethod());

            string[] usingStrings = [typeof(BaseAgent).Namespace!, nameof(System), typeof(IDictionary<string, dynamic>).Namespace!];

            var usings = usingStrings.Select(u => SyntaxFactory.UsingDirective(
                            SyntaxFactory.IdentifierName(u)
                        )).ToArray();

            var @namespace = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName(GeneratedNamespace))
            .AddMembers(@class)
            .AddUsings(usings);


            Root = @namespace;
        }

        public override void EnterClass_def([NotNull] SctParser.Class_defContext context) => _isInAgent = true;

        public override void ExitClass_def([NotNull] SctParser.Class_defContext context)
        {
            _isInAgent = false;
            // Pop functions, decorators, and states, but throw away parameter list, as it is not relevant post-type checking
            // All constructors are equal, so we can just create a custom one
            var members = _stack.PopUntil<ParameterListSyntax, MemberDeclarationSyntax>(out var fields);
            var className = TranslatorUtils.GetMangledStringName(context.ID().GetText());

            var @class = SyntaxFactory.ClassDeclaration(className)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(nameof(BaseAgent))))
            .AddMembers(TranslatorUtils.CreateClassFields(fields))
            .AddMembers(TranslatorUtils.CreateConstructor(className))
            .AddMembers(members)
            .AddMembers(TranslatorUtils.CreateUpdateMethod(_stateNames));

            _stateNames = new(); // reset state names

            _stack.Push(@class);
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
            var @params = context.ID().Select(id => TranslatorUtils.GetMangledName(id.GetText()))
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
            .Select((arg, i) => (expr: arg, name: TranslatorUtils.GetMangledStringName(context.ID(i).GetText())))
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

            var mangledName = TranslatorUtils.GetMangledName(context.ID().GetText());

            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), // all decorators return bool
                mangledName
            )
            .WithParameterList(TranslatorUtils.WithContextParameter([])) // all decorators take 0 arguments;
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
            .WithBody(childBlock);

            _stack.Push(method);
        }

        public override void ExitFunction([NotNull] SctParser.FunctionContext context)
        {
            var childBlock = _stack.Pop<BlockSyntax>();
            var @params = TranslatorUtils.WithContextParameter(_stack.Pop<ParameterListSyntax>());
            var mangledName = TranslatorUtils.GetMangledName(context.ID().GetText());

            var method = SyntaxFactory.MethodDeclaration(
                TypeTable.GetTypeNode(context.type().GetText()),
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
            var mangledName = TranslatorUtils.GetMangledName(context.ID().GetText());
            // push decorator to the stack as method call
            var state = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(mangledName),
                TranslatorUtils.WithContextArgument([])
            );

            _stack.Push(state);
        }

        public override void ExitVariableDeclaration([NotNull] SctParser.VariableDeclarationContext context)
        {
            var expression = _stack.Pop<ExpressionSyntax>();

            var mangledName = TranslatorUtils.GetMangledName(context.ID().GetText());

            var variable = SyntaxFactory.VariableDeclaration(
                TypeTable.GetTypeNode(context.type().GetText()) // set type
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
            var mangledName = TranslatorUtils.GetMangledName(context.ID().GetText());
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

            var name = TranslatorUtils.GetMangledName(context.ID().GetText());
            _stateNames.Add(name.Text);
            var method = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                name
            )
            .WithParameterList(TranslatorUtils.WithContextParameter([])) // state only takes context
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
            var text = context.literal().GetText();
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
            var id = TranslatorUtils.GetMangledName(context.ID().GetText());
            var idExp = SyntaxFactory.IdentifierName(id);
            _stack.Push(idExp);
        }

        public override void ExitBinaryExpression([NotNull] SctParser.BinaryExpressionContext context)
        {
            var exp2 = _stack.Pop<ExpressionSyntax>();
            var exp1 = _stack.Pop<ExpressionSyntax>();

            // switch expression does not need to descructure the context,
            // so we determine it based on 'when'
            var @operator = context.op.Type switch
            {
                { } op when op == SctLexer.PLUS => SyntaxKind.AddExpression,
                { } op when op == SctLexer.MINUS => SyntaxKind.SubtractExpression,
                { } op when op == SctLexer.MULT => SyntaxKind.MultiplyExpression,
                { } op when op == SctLexer.DIV => SyntaxKind.DivideExpression,
                { } op when op == SctLexer.MOD => SyntaxKind.ModuloExpression,
                _ => SyntaxKind.None
            };

            var binaryExpression = SyntaxFactory.ParenthesizedExpression(
                SyntaxFactory.BinaryExpression(@operator, exp1, exp2)
            );
            _stack.Push(binaryExpression);
        }

        public override void ExitBooleanExpression([NotNull] SctParser.BooleanExpressionContext context)
        {
            var exp2 = _stack.Pop<ExpressionSyntax>();
            var exp1 = _stack.Pop<ExpressionSyntax>();

            var @operator = context.op.Type switch
            {
                { } op when op == SctLexer.GT => SyntaxKind.GreaterThanExpression,
                { } op when op == SctLexer.LT => SyntaxKind.LessThanExpression,
                { } op when op == SctLexer.GTE => SyntaxKind.GreaterThanOrEqualExpression,
                { } op when op == SctLexer.LTE => SyntaxKind.LessThanOrEqualExpression,
                { } op when op == SctLexer.EQ => SyntaxKind.EqualsExpression,
                { } op when op == SctLexer.NEQ => SyntaxKind.NotEqualsExpression,
                { } op when op == SctLexer.AND => SyntaxKind.LogicalAndExpression,
                { } op when op == SctLexer.OR => SyntaxKind.LogicalOrExpression,
                _ => SyntaxKind.None
            };

            // convert to conditional, as booleans do not exist in SCT
            var expression = SyntaxFactory.BinaryExpression(@operator, exp1, exp2);
            // Add parenthesis for debugging / testing readability.
            // This is not required for correct precedence, as ternaries have lowest priority
            var parenthesized = SyntaxFactory.ParenthesizedExpression(expression);
            var condition = SyntaxFactory.ConditionalExpression(parenthesized, SctTrue, SctFalse);
            var parenthesizedCondition = SyntaxFactory.ParenthesizedExpression(condition);
            _stack.Push(parenthesizedCondition);
        }

        public override void ExitParenthesisExpression([NotNull] SctParser.ParenthesisExpressionContext context)
        {
            var node = _stack.Pop<ExpressionSyntax>();
            _stack.Push(SyntaxFactory.ParenthesizedExpression(node));
        }

        public override void ExitTypecastExpression([NotNull] SctParser.TypecastExpressionContext context)
        {
            var node = _stack.Pop<ExpressionSyntax>();
            var type = TypeTable.GetTypeNode(context.type().GetText());
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
            var args = TranslatorUtils.WithContextArgument(_stack.Pop<ArgumentListSyntax>());
            var call = TranslatorUtils.GetFunction(context.ID().GetText(), args);
            _stack.Push(call);
        }

        public override void ExitAgent_create([NotNull] SctParser.Agent_createContext context)
        {
            var fields = _stack.Pop<ObjectCreationExpressionSyntax>();
            var state = TranslatorUtils.GetMangledStringName(context.ID(1).GetText());
            var type = TranslatorUtils.GetMangledStringName(context.ID(0).GetText());

            var agent = SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.ParseTypeName(type)
            )
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
                    // call ctx.AgentHandler.CreateAgent
                    TranslatorUtils.BuildAccessor(
                        nameof(IAgentHandler.CreateAgent), ContextIdentifier, SyntaxFactory.Identifier(nameof(IRuntimeContext.AgentHandler))
                    ),
                    // with agent as argument
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList([SyntaxFactory.Argument(agent)])
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
                SyntaxFactory.Literal(TranslatorUtils.GetMangledStringName(context.ID(0).GetText()))
            ));
            var state = context.QUESTION() switch
            {
                null => SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(TranslatorUtils.GetMangledStringName(context.ID(1).GetText()))
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
            if (_stack.TryPop<ExpressionSyntax>(out var expr))
            {
                @return = @return.WithExpression(expr);
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
            _ = _stack.TryPop<ElseClauseSyntax>(out var @else);
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
                    TranslatorUtils.BuildAccessor(nameof(IRuntimeContext.ExitRuntime), ContextIdentifier)
                )
            ));

            // return true any time we exit
            _stack.Push(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)));
        }

        public override void ExitEnter([NotNull] SctParser.EnterContext context)
        {
            var state = SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(TranslatorUtils.GetMangledStringName(context.ID().GetText()))
            );
            var invocation = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName(BaseAgent.EnterMethodName),
                    TranslatorUtils.WithContextArgument([SyntaxFactory.Argument(state)])
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
    }
}

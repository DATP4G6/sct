using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Compiler.Syntax;
using Sct.Extensions;
using Sct.Runtime;

namespace Sct.Compiler.Translator
{
    public class SctTranslator : SctBaseSyntaxVisitor<CSharpSyntaxNode>
    {
        public const string GeneratedNamespace = "SctGenerated";
        public const string GeneratedGlobalClass = "GlobalClass";
        public const string RunSimulationFunctionName = "RunSimulation";

        public static readonly SyntaxToken ContextIdentifier = SyntaxFactory.Identifier("ctx");

        private bool _isInSpecies = false;

        public override CSharpSyntaxNode Visit(SctProgramSyntax node)
        {
            var members = node.Children
                .Select(c => c.Accept(this))
                .Cast<MemberDeclarationSyntax>()
                .ToArray();

            var @class = SyntaxFactory
                .ClassDeclaration(GeneratedGlobalClass)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(members)
                .AddMembers(TranslatorUtils.MakeRunMethod());

            string[] usingStrings = [typeof(BaseAgent).Namespace!, nameof(System), typeof(IDictionary<string, dynamic>).Namespace!];

            var usings = usingStrings
                .Select(u => SyntaxFactory.UsingDirective(
                    SyntaxFactory.IdentifierName(u)
                )).ToArray();

            return SyntaxFactory
                .NamespaceDeclaration(SyntaxFactory.ParseName(GeneratedNamespace))
                .AddMembers(@class)
                .AddUsings(usings);
        }

        public override CSharpSyntaxNode Visit(SctFunctionSyntax node)
        {
            var parameters = node.Parameters
                .Select(p => p.Accept(this))
                .Cast<ParameterSyntax>();
            var returnType = TranslatorUtils.GetType(node.ReturnType);
            var name = TranslatorUtils.GetMangledName(node.Id);
            var body = (BlockSyntax)node.Block.Accept(this);

            var method = SyntaxFactory.MethodDeclaration(returnType, name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithParameterList(TranslatorUtils.WithContextParameter(parameters))
                .WithBody(body);

            if (!_isInSpecies) // all global functions are static
            {
                method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
            }

            return method;
        }

        public override CSharpSyntaxNode Visit(SctClassSyntax node)
        {
            _isInSpecies = true;

            var decorators = node.Decorators
                .Select(d => d.Accept(this))
                .Cast<MemberDeclarationSyntax>();
            var functions = node.Functions
                .Select(f => f.Accept(this))
                .Cast<MemberDeclarationSyntax>();
            var states = node.States
                .Select(s => s.Accept(this))
                .Cast<MemberDeclarationSyntax>();

            var members = decorators
                .Concat(functions)
                .Concat(states)
                .ToArray();

            var fields = SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(node.Parameters
                    .Select(p => p.Accept(this))
                    .Cast<ParameterSyntax>()
                )
            );

            var name = TranslatorUtils.GetMangledStringName(node.Id);

            var stateNames = node.States
                .Select(s => TranslatorUtils.GetMangledStringName(s.Id));

            var @class = SyntaxFactory.ClassDeclaration(name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(nameof(BaseAgent))))
                .AddMembers(TranslatorUtils.CreateClassFields(fields))
                .AddMembers(TranslatorUtils.CreateConstructor(name))
                .AddMembers(members)
                .AddMembers(TranslatorUtils.CreateUpdateMethod(stateNames));

            _isInSpecies = false;

            return @class;
        }

        public override CSharpSyntaxNode Visit(SctParameterSyntax node) =>
             SyntaxFactory.Parameter(TranslatorUtils.GetMangledName(node.Id))
                .WithType(TranslatorUtils.GetType(node.Type));

        public override CSharpSyntaxNode Visit(SctStateSyntax node)
        {
            var name = TranslatorUtils.GetMangledName(node.Id);
            var stateBlock = (BlockSyntax)node.Block.Accept(this);

            var logic = stateBlock.Statements.Add(
                SyntaxFactory.ReturnStatement(
                    SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression))
            );

            // invoke decorators
            var decorators = node.Decorations.Select(decor => SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName(TranslatorUtils.GetMangledName(decor)),
                    TranslatorUtils.WithContextArgument([])
                ));
            // wrap all decorator invocations in if statements to exit early
            var ifs = decorators.Select(decor => SyntaxFactory.IfStatement(
                    decor,
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)
                    )
                ))
                .Cast<StatementSyntax>();

            // create new body with decorators first, then state logic
            var body = SyntaxFactory.Block(ifs.Concat(logic));

            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    name
                ).WithParameterList(TranslatorUtils.WithContextParameter([])) // state only takes context
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .WithBody(body);
        }

        public override CSharpSyntaxNode Visit(SctDecoratorSyntax node)
        {
            var childBlock = (BlockSyntax)node.Block.Accept(this);
            childBlock = childBlock.AddStatements(SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)));
            var mangledName = TranslatorUtils.GetMangledName(node.Id);

            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), // all decorators return bool
                    mangledName
                ).WithParameterList(TranslatorUtils.WithContextParameter([])) // all decorators take 0 arguments;
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                .WithBody(childBlock);
        }

        public override CSharpSyntaxNode Visit(SctBlockStatementSyntax node) =>
            SyntaxFactory.Block(node.Children
                .Select(c => c.Accept(this))
                .Cast<StatementSyntax>()
            );

        public override CSharpSyntaxNode Visit(SctExpressionStatementSyntax node) =>
            SyntaxFactory.ExpressionStatement((ExpressionSyntax)node.Expression.Accept(this));

        public override CSharpSyntaxNode Visit(SctDeclarationStatementSyntax node)
        {
            var expression = (ExpressionSyntax)node.Expression.Accept(this);
            var mangledName = TranslatorUtils.GetMangledName(node.Id);
            var variable = SyntaxFactory.VariableDeclaration(
                    TranslatorUtils.GetType(node.Type) // set type
                )
                .AddVariables(
                    SyntaxFactory.VariableDeclarator(mangledName) // set name
                        .WithInitializer(SyntaxFactory.EqualsValueClause(expression)) // equal to expression
                );

            // convert to statement
            return SyntaxFactory.LocalDeclarationStatement(variable);
        }

        public override CSharpSyntaxNode Visit(SctAssignmentStatementSyntax node)
        {
            var expression = (ExpressionSyntax)node.Expression.Accept(this);
            var mangledName = TranslatorUtils.GetMangledName(node.Id);
            return SyntaxFactory.ExpressionStatement( // convert to expression
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(mangledName), // assign name
                    expression // to expression
                )
            );
        }

        public override CSharpSyntaxNode Visit(SctWhileStatementSyntax node)
        {
            var expression = (ExpressionSyntax)node.Expression.Accept(this);
            var condition = TranslatorUtils.IntToBool(expression);
            var body = (BlockSyntax)node.Block.Accept(this);
            return SyntaxFactory.WhileStatement(condition, body);
        }

        public override CSharpSyntaxNode Visit(SctIfStatementSyntax node)
        {
            var @if = SyntaxFactory.IfStatement(
                TranslatorUtils.IntToBool((ExpressionSyntax)node.Expression.Accept(this)),
                (BlockSyntax)node.Then.Accept(this)
            );
            return node.Else is null
                ? @if
                : @if.WithElse(SyntaxFactory.ElseClause((StatementSyntax)node.Else.Accept(this)));
        }

        public override CSharpSyntaxNode Visit(SctElseStatementSyntax node) =>
            node.Block.Accept(this);

        public override CSharpSyntaxNode Visit(SctBreakStatementSyntax node) =>
            SyntaxFactory.BreakStatement();


        public override CSharpSyntaxNode Visit(SctContinueStatementSyntax node) =>
            SyntaxFactory.ContinueStatement();

        public override CSharpSyntaxNode Visit(SctReturnStatementSyntax node)
        {
            var @return = SyntaxFactory.ReturnStatement();
            return node.Expression is null
                ? @return
                : @return.WithExpression((ExpressionSyntax)node.Expression.Accept(this));
        }

        public override CSharpSyntaxNode Visit(SctDestroyStatementSyntax node) =>
            SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));

        public override CSharpSyntaxNode Visit(SctExitStatementSyntax node)
        {
            // call ExitRuntime on context
            var invocation = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    TranslatorUtils.BuildAccessor(nameof(IRuntimeContext.ExitRuntime), ContextIdentifier)
                )
            );
            // return true any time we exit
            var @return = SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));

            return SyntaxFactory.Block(invocation, @return);
        }

        public override CSharpSyntaxNode Visit(SctEnterStatementSyntax node)
        {
            var state = SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(TranslatorUtils.GetMangledStringName(node.Id))
            );
            var invocation = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName(BaseAgent.EnterMethodName),
                    TranslatorUtils.WithContextArgument([SyntaxFactory.Argument(state)])
                )
            );
            var @return = SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));

            // invoke Enter of BaseAgent, and return to prevent further execution
            return SyntaxFactory.Block(invocation, @return);
        }

        public override CSharpSyntaxNode Visit(SctCreateStatementSyntax node)
        {
            var state = TranslatorUtils.GetMangledStringName(node.Agent.StateName);
            var type = TranslatorUtils.GetMangledStringName(node.Agent.ClassName);
            var fields = (ObjectCreationExpressionSyntax)node.Agent.Accept(this);

            var agentFields = SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.ParseTypeName(TranslatorUtils.GetMangledStringName(node.Agent.ClassName))
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

            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    // call ctx.AgentHandler.CreateAgent
                    TranslatorUtils.BuildAccessor(
                        nameof(IAgentHandler.CreateAgent), ContextIdentifier, SyntaxFactory.Identifier(nameof(IRuntimeContext.AgentHandler))
                    ),
                    // with agent as argument
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList([SyntaxFactory.Argument(agentFields)])
                    )
                )
            );
        }

        public override CSharpSyntaxNode Visit(SctAgentExpressionSyntax node)
        {
            var className = SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(TranslatorUtils.GetMangledStringName(node.ClassName))
            );
            var stateName = SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(TranslatorUtils.GetMangledStringName(node.StateName))
            );

            var fields = node.Fields
                .Select(f => f.Accept(this))
                .Cast<ObjectCreationExpressionSyntax>()
                .ToArray();

            var arrayExpression = SyntaxFactory.ArrayCreationExpression(
                SyntaxFactory.ArrayType(SyntaxFactory.ParseTypeName(typeof(KeyValuePair<string, dynamic>).GenericName() + "[]")),
                SyntaxFactory.InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(
                        fields
                    )
                )
            );

            return SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.ParseTypeName(typeof(Dictionary<string, dynamic>).GenericName())
                ).WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[] {
                            SyntaxFactory.Argument(
                                arrayExpression
                            )
                        })
                    ));
        }

        public override CSharpSyntaxNode Visit(SctPredicateExpressionSyntax node)
        {
            // list of KeyValuePairs
            var fields = node.Fields
                .Select(f => f.Accept(this))
                .Cast<ObjectCreationExpressionSyntax>()
                .ToArray();
            // Single argument of KeyValuePair[] with all KV pairs as arguments
            var arrayExpression = SyntaxFactory.ArrayCreationExpression(
                SyntaxFactory.ArrayType(SyntaxFactory.ParseTypeName(typeof(KeyValuePair<string, dynamic>).GenericName() + "[]")),
                SyntaxFactory.InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(
                        fields
                    )
                )
            );
            // dictionary with KeyValePair[] as argument
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

            var className = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(TranslatorUtils.GetMangledStringName(node.ClassName))
            ));

            var state = node.StateName switch
            {
                // if state is wildcard
                null => SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                        SyntaxKind.NullLiteralExpression
                    )),
                // if state is specified
                _ => SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(TranslatorUtils.GetMangledStringName(node.StateName))
                    )),
            };

            return SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.ParseTypeName(nameof(QueryPredicate))
            ).WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList([
                        className,
                        state,
                        SyntaxFactory.Argument(dictionary)
                    ])
                )
            );
        }

        public override CSharpSyntaxNode Visit(SctNamedArgumentSyntax node)
        {
            return SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.ParseTypeName(typeof(KeyValuePair<string, dynamic>).GenericName())
                ).WithArgumentList(SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                    // set key to ID
                        SyntaxFactory.Argument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(TranslatorUtils.GetMangledStringName(node.Id)))
                            ),
                            SyntaxFactory.Argument((ExpressionSyntax)node.Expression.Accept(this)) // set value to expression
                    })
                ));
        }

        public override CSharpSyntaxNode Visit(SctBinaryExpressionSyntax node)
        {
            var @operator = node.Op switch
            {
                SctBinaryOperator.Plus => SyntaxKind.AddExpression,
                SctBinaryOperator.Minus => SyntaxKind.SubtractExpression,
                SctBinaryOperator.Mult => SyntaxKind.MultiplyExpression,
                SctBinaryOperator.Div => SyntaxKind.DivideExpression,
                SctBinaryOperator.Mod => SyntaxKind.ModuloExpression,
                _ => SyntaxKind.None
            };

            var exp1 = (ExpressionSyntax)node.Left.Accept(this);
            var exp2 = (ExpressionSyntax)node.Right.Accept(this);

            return SyntaxFactory.ParenthesizedExpression(
                SyntaxFactory.BinaryExpression(@operator, exp1, exp2)
            );
        }

        public override CSharpSyntaxNode Visit(SctBooleanExpressionSyntax node)
        {
            var @operator = node.Op switch
            {
                SctBooleanOperator.Gt => SyntaxKind.GreaterThanExpression,
                SctBooleanOperator.Lt => SyntaxKind.LessThanExpression,
                SctBooleanOperator.Gte => SyntaxKind.GreaterThanOrEqualExpression,
                SctBooleanOperator.Lte => SyntaxKind.LessThanOrEqualExpression,
                SctBooleanOperator.Eq => SyntaxKind.EqualsExpression,
                SctBooleanOperator.Neq => SyntaxKind.NotEqualsExpression,
                SctBooleanOperator.And => SyntaxKind.LogicalAndExpression,
                SctBooleanOperator.Or => SyntaxKind.LogicalOrExpression,
                _ => SyntaxKind.None
            };
            var exp1 = (ExpressionSyntax)node.Left.Accept(this);
            var exp2 = (ExpressionSyntax)node.Right.Accept(this);

            // If the operator is && or ||, we need to convert the operands to bools
            (exp1, exp2) = node.Op switch
            {
                SctBooleanOperator.And or SctBooleanOperator.Or => (TranslatorUtils.IntToBool(exp1), TranslatorUtils.IntToBool(exp2)),
                _ => (exp1, exp2)
            };

            // convert to conditional, as booleans do not exist in SCT
            var expression = SyntaxFactory.BinaryExpression(@operator, exp1, exp2);
            // Add parenthesis for debugging / testing readability.
            // This is not required for correct precedence, as ternaries have lowest priority
            var parenthesized = SyntaxFactory.ParenthesizedExpression(expression);
            var condition = TranslatorUtils.BoolToInt(parenthesized);
            return SyntaxFactory.ParenthesizedExpression(condition);
        }

        public override CSharpSyntaxNode Visit(SctTypecastExpressionSyntax node)
        {
            var type = TranslatorUtils.GetType(node.Type);
            var expression = (ExpressionSyntax)node.Expression.Accept(this);
            return SyntaxFactory.CastExpression(type, expression);
        }

        public override CSharpSyntaxNode Visit(SctParenthesisExpressionSyntax node) =>
            SyntaxFactory.ParenthesizedExpression((ExpressionSyntax)node.Expression.Accept(this));

        public override CSharpSyntaxNode Visit(SctNotExpressionSyntax node)
        {
            var @operator = SyntaxKind.EqualsExpression;
            var expression = (ExpressionSyntax)node.Expression.Accept(this);
            // compare with 0, and convert to SctBoolean via conditional
            var condition = SyntaxFactory.BinaryExpression(@operator, expression, TranslatorUtils.SctFalse);
            return TranslatorUtils.BoolToInt(condition);
        }
        public override CSharpSyntaxNode Visit(SctUnaryMinusExpressionSyntax node)
        {
            var expression = (ExpressionSyntax)node.Expression.Accept(this);
            return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression, expression);
        }
        public override CSharpSyntaxNode Visit(SctCallExpressionSyntax node)
        {
            var expressions = node.Expressions
                .Select(e => (ExpressionSyntax)e.Accept(this))
                .Select(SyntaxFactory.Argument);
            var args = TranslatorUtils.WithContextArgument(expressions);
            return TranslatorUtils.GetFunction(node.Target, args);
        }

        public override CSharpSyntaxNode Visit(SctIdExpressionSyntax node) =>
            SyntaxFactory.IdentifierName(TranslatorUtils.GetMangledName(node.Id));

        public override CSharpSyntaxNode Visit(SctTypeSyntax node) =>
            TranslatorUtils.GetType(node);

        public override CSharpSyntaxNode Visit(SctLiteralExpressionSyntax<long> node) =>
            SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(node.Value)
            );

        public override CSharpSyntaxNode Visit(SctLiteralExpressionSyntax<double> node) =>
            SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(node.Value)
            );
    }
}

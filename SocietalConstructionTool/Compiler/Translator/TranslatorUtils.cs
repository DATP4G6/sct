using System.Globalization;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Compiler.Exceptions;
using Sct.Compiler.Syntax;
using Sct.Extensions;
using Sct.Runtime;

namespace Sct.Compiler.Translator
{
    public static class TranslatorUtils
    {
        private const string NAME_MANGLE_PREFIX = "__sct_";
        private static readonly SyntaxToken RunSimulationIdentifier = SyntaxFactory.Identifier(SctTranslator.RunSimulationFunctionName);
        private static readonly IdentifierNameSyntax ContextIdentifierName = SyntaxFactory.IdentifierName(SctTranslator.ContextIdentifier);
        private static readonly SyntaxToken QueryHandlerIdentifier = SyntaxFactory.Identifier(nameof(IRuntimeContext.QueryHandler));
        private static readonly SyntaxToken StdlibIdentifier = SyntaxFactory.Identifier(nameof(Stdlib));

        // boolean values are either 0 or 1
        public static readonly LiteralExpressionSyntax SctTrue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1L));
        public static readonly LiteralExpressionSyntax SctFalse = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0L));

        // Due to the way static fields are initialized, this field MUST be placed after the identifiers
        // A full day has gone to waste debugging this...
        private static readonly Dictionary<string, MemberAccessExpressionSyntax> Types = new()
        {
            { "rand", BuildAccessor(nameof(Stdlib.Rand), StdlibIdentifier) },
            { "seed", BuildAccessor(nameof(Stdlib.Seed), StdlibIdentifier) },
            { "exists", BuildAccessor(nameof(IQueryHandler.Exists), SctTranslator.ContextIdentifier, QueryHandlerIdentifier) },
            { "count", BuildAccessor(nameof(IQueryHandler.Count), SctTranslator.ContextIdentifier, QueryHandlerIdentifier) },
        };

        public static TypeSyntax GetType(SctTypeSyntax type)
        {
            return type.Type switch
            {
                SctType.Int => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)),
                SctType.Float => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword)),
                SctType.Predicate => SyntaxFactory.ParseTypeName(nameof(QueryPredicate)),
                SctType.Void => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                _ => throw new InvalidTypeException($"Type {type.Type} does not exist"),
            };
        }

        public static MemberAccessExpressionSyntax BuildAccessor(string accessor, params SyntaxToken[] members)
        {
            return BuildAccessor(SyntaxFactory.Identifier(accessor), members);
        }

        /// <summary>
        /// Builds a MemberAccessExpressionSyntax like Foo.Bar.Baz
        /// </summary>
        /// <param name="accessor">The last accessor that must be called / invoked</param>
        /// <param name="members">all members leading up to it, in order</param>
        /// <returns>a MemberAccessExpressionSyntax corresponding to the parameters</returns>
        /// <exception cref="ArgumentException">If no `members` are provided</exception>
        public static MemberAccessExpressionSyntax BuildAccessor(SyntaxToken accessor, params SyntaxToken[] members)
        {
            if (members.Length == 0)
            {
                throw new ArgumentException("At least one member must be provided");
            }

            // base case
            if (members.Length == 1)
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(members[0]),
                    SyntaxFactory.IdentifierName(accessor)
                );
            }

            // left-recursive call
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                BuildAccessor(members[^1], members[..^1]),
                SyntaxFactory.IdentifierName(accessor)
            );
        }

        public static string GetMangledStringName(string n) => NAME_MANGLE_PREFIX + n;

        /// <summary>
        /// Get the mangled name of a identifier
        /// </summary>
        /// <param name="identifier">identifier as string</param>
        /// <returns>IdentifierNameSyntax with the mangled name</returns>
        public static SyntaxToken GetMangledName(string identifier)
        {
            return SyntaxFactory.Identifier(GetMangledStringName(identifier));
        }

        /// <summary>
        /// Get the (possible) mangled name of a function call
        /// If the identifier is a reserved keyword, it will map accordingly
        /// </summary>
        /// <param name="identifier">identifier from the sct source</param>
        /// <param name="args">Arguments for the method call - nullable</param>
        /// <returns>InvocationExpressionSyntax</returns>
        public static InvocationExpressionSyntax GetFunction(string identifier, ArgumentListSyntax? args)
        {
            if (Types.TryGetValue(identifier, out MemberAccessExpressionSyntax? function))
            {
                return SyntaxFactory.InvocationExpression(function, args ?? SyntaxFactory.ArgumentList());
            }

            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(GetMangledName(identifier)),
                args ?? SyntaxFactory.ArgumentList()
            );
        }

        public static InvocationExpressionSyntax GetFunction(string identifier, params ArgumentSyntax[]? args)
        {
            return GetFunction(identifier, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(args)));
        }

        /// <summary>
        /// Returns a <see cref="ParameterListSyntax"/> with an <see cref="IRuntimeContext"/> <code>ctx</code> parameter added.
        /// </summary>
        public static ParameterListSyntax WithContextParameter(ParameterListSyntax p) => WithContextParameter(p.Parameters);

        /// <summary>
        /// Returns a <see cref="ParameterListSyntax"/> with an <see cref="IRuntimeContext"/> <code>ctx</code> parameter added.
        /// </summary>
        public static ParameterListSyntax WithContextParameter(IEnumerable<ParameterSyntax> p)
        {
            // create new parameter list
            return SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(
                    p.Prepend( // prepend context to the original parameters
                        SyntaxFactory.Parameter(SctTranslator.ContextIdentifier)
                        .WithType(SyntaxFactory.ParseTypeName(nameof(IRuntimeContext)))
                    )
                )
            );
        }

        public static ArgumentListSyntax WithContextArgument(IEnumerable<ArgumentSyntax> a) => SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(a.Prepend(
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(SctTranslator.ContextIdentifier))
                    ))
                );
        public static ArgumentListSyntax WithContextArgument(ArgumentListSyntax a) => WithContextArgument(a.Arguments);

        public static ConstructorDeclarationSyntax CreateConstructor(string className)
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

        public static MethodDeclarationSyntax CreateUpdateMethod(IEnumerable<string> stateNames)
        {
            var @switch = SyntaxFactory.SwitchExpression(
                SyntaxFactory.IdentifierName(nameof(BaseAgent.State)),
                // Add cases for each state in stateNames, calling the corresponding method of the same name
                SyntaxFactory.SeparatedList(
                    stateNames.Select(stateName => SyntaxFactory.SwitchExpressionArm(
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
                );

            var discardAssignment = SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName("_"),
                @switch
            ));
            var body = SyntaxFactory.Block().AddStatements(discardAssignment);

            var updateMethod = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                nameof(BaseAgent.Update)
            )
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword))
            .WithParameterList(WithContextParameter([]))
            .WithBody(body);

            return updateMethod;
        }

        public static MethodDeclarationSyntax MakeRunMethod()
        {
            var runtimeId = SyntaxFactory.Identifier("runtime");
            var runtime = SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName(nameof(Runtime))
                )
                .AddVariables(
                    SyntaxFactory.VariableDeclarator(
                        runtimeId
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

            var setup = SyntaxFactory.ExpressionStatement(
                GetFunction("setup",
                    SyntaxFactory.Argument(ContextIdentifierName)
                )
            );

            var run = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    BuildAccessor(nameof(IRuntime.Run), runtimeId),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList([SyntaxFactory.Argument(ContextIdentifierName)])
                    )
                )
            );

            var runtimeParameter = SyntaxFactory.Parameter(SctTranslator.ContextIdentifier).WithType(SyntaxFactory.ParseTypeName(nameof(IRuntimeContext)));

            var runMethod = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                RunSimulationIdentifier
            )
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .AddParameterListParameters(runtimeParameter)
            .WithBody(SyntaxFactory.Block()
            .AddStatements(runtime, setup, run));

            return runMethod;
        }

        /// <summary>
        /// Create getters and setters for all fields in a class
        /// </summary>
        /// <param name="fields">Fields in the class</param>
        /// <returns></returns>
        public static PropertyDeclarationSyntax[] CreateClassFields(ParameterListSyntax fields)
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

        public static ExpressionSyntax BoolToInt(ExpressionSyntax exp) => SyntaxFactory.ConditionalExpression(exp, SctTrue, SctFalse);

        /// <summary>
        /// Converts an expression to a boolean condion by comparing it to 0
        /// </summary>
        /// <param name="expression">Expression to be compared</param>
        /// <returns>expression != 0</returns>
        public static BinaryExpressionSyntax IntToBool(ExpressionSyntax expression) => SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, SyntaxFactory.ParenthesizedExpression(expression), SctFalse);
    }
}

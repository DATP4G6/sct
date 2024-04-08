using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Sct.Runtime;

namespace Sct.Compiler.Translator
{
    public static class TranslatorUtils
    {
        private const string NAME_MANGLE_PREFIX = "__sct_";
        private static readonly SyntaxToken ContextIdentifier = SyntaxFactory.Identifier("ctx");
        private static readonly SyntaxToken AgentHandlerIdentifier = SyntaxFactory.Identifier(nameof(IRuntimeContext.AgentHandler));
        private static readonly SyntaxToken QueryHandlerIdentifier = SyntaxFactory.Identifier(nameof(IRuntimeContext.QueryHandler));
        private static readonly SyntaxToken StdlibIdentifier = SyntaxFactory.Identifier(nameof(Stdlib));

        // Due to the way static fields are initialized, this field MUST be placed after the identifiers
        // A full day has gone to waste debugging this...
        private static readonly Dictionary<string, MemberAccessExpressionSyntax> Types = new()
        {
            { "rand", BuildAccessor(nameof(Stdlib.Rand), StdlibIdentifier) },
            { "seed", BuildAccessor(nameof(Stdlib.Seed), StdlibIdentifier) },
            { "exists", BuildAccessor(nameof(IQueryHandler.Exists), ContextIdentifier, QueryHandlerIdentifier) },
            { "count", BuildAccessor(nameof(IQueryHandler.Count), ContextIdentifier, QueryHandlerIdentifier) },
        };

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
    }
}

namespace Sct.Compiler.Syntax
{
    public class SctBaseSyntaxVisitor<T>
    {
        protected virtual T VisitChildren(SctSyntax node) => node.Children.Select(c => c.Accept(this)).Aggregate(DefaultResult, AggregateResult);
        protected virtual T DefaultResult => default!;
        protected virtual T AggregateResult(T aggregate, T nextResult) => nextResult;

        // Tell the visitor to use the more specific Visit overload
        // Casting to dynamic forces C# to look at the concrete class, even if current type is abstract
        public virtual T Visit(SctSyntax node) => Visit((dynamic)node);

        public virtual T Visit(SctNamedArgumentSyntax node) => VisitChildren(node);
        public virtual T Visit(SctParameterSyntax node) => VisitChildren(node);
        public virtual T Visit(SctProgramSyntax node) => VisitChildren(node);
        public virtual T Visit(SctTypeSyntax node) => VisitChildren(node);
        public virtual T Visit(SctClassSyntax node) => VisitChildren(node);
        public virtual T Visit(SctDecoratorSyntax node) => VisitChildren(node);
        public virtual T Visit(SctDefinitionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctFunctionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctStateSyntax node) => VisitChildren(node);
        public virtual T Visit(SctAgentExpressionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctBinaryExpressionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctBooleanExpressionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctCallExpressionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctExpressionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctIdExpressionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctLiteralExpressionSyntax<long> node) => VisitChildren(node);
        public virtual T Visit(SctLiteralExpressionSyntax<double> node) => VisitChildren(node);
        public virtual T Visit(SctNotExpressionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctParenthesisExpressionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctPredicateExpressionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctTypecastExpressionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctUnaryMinusExpressionSyntax node) => VisitChildren(node);
        public virtual T Visit(SctAssignmentStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctBlockStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctBreakStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctContinueStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctCreateStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctDeclarationStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctDestroyStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctElseStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctEnterStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctExitStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctExpressionStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctIfStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctReturnStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctStatementSyntax node) => VisitChildren(node);
        public virtual T Visit(SctWhileStatementSyntax node) => VisitChildren(node);
    }
}

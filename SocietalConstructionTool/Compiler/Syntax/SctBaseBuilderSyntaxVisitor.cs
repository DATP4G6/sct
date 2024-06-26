namespace Sct.Compiler.Syntax
{
    public class SctBaseBuilderSyntaxVisitor : SctBaseSyntaxVisitor<SctSyntax>
    {
        public override SctSyntax Visit(SctProgramSyntax node)
        {
            var functions = node.Functions.Select(f => f.Accept(this)).Cast<SctFunctionSyntax>();
            var classes = node.Classes.Select(c => c.Accept(this)).Cast<SctClassSyntax>();
            return new SctProgramSyntax(node.Context, functions, classes);
        }

        public override SctSyntax Visit(SctFunctionSyntax node)
        {
            var parameters = node.Parameters.Select(p => p.Accept(this)).Cast<SctParameterSyntax>();
            var body = (SctBlockStatementSyntax)node.Block.Accept(this);
            var type = (SctTypeSyntax)node.ReturnType.Accept(this);
            return new SctFunctionSyntax(node.Context, node.Id, parameters, type, body);
        }

        public override SctSyntax Visit(SctClassSyntax node)
        {
            var parameters = node.Parameters.Select(f => f.Accept(this)).Cast<SctParameterSyntax>();
            var states = node.States.Select(s => s.Accept(this)).Cast<SctStateSyntax>();
            var functions = node.Functions.Select(m => m.Accept(this)).Cast<SctFunctionSyntax>();
            var decorators = node.Decorators.Select(d => d.Accept(this)).Cast<SctDecoratorSyntax>();
            return new SctClassSyntax(node.Context, node.Id, parameters, decorators, functions, states);
        }

        public override SctSyntax Visit(SctParameterSyntax node)
        {
            var type = (SctTypeSyntax)node.Type.Accept(this);
            return new SctParameterSyntax(node.Context, type, node.Id);
        }

        public override SctSyntax Visit(SctStateSyntax node)
        {
            // Clone the decorations' list
            var decorations = node.Decorations.Select(d => d);
            var body = (SctBlockStatementSyntax)node.Block.Accept(this);
            return new SctStateSyntax(node.Context, node.Id, decorations, body);
        }

        public override SctSyntax Visit(SctDecoratorSyntax node)
        {
            var body = (SctBlockStatementSyntax)node.Block.Accept(this);
            return new SctDecoratorSyntax(node.Context, node.Id, body);
        }

        public override SctSyntax Visit(SctBlockStatementSyntax node)
        {
            var statements = node.Statements.Select(s => s.Accept(this)).Cast<SctStatementSyntax>();
            return new SctBlockStatementSyntax(node.Context, statements);
        }

        public override SctSyntax Visit(SctExpressionStatementSyntax node)
        {
            var expression = (SctExpressionSyntax)node.Expression.Accept(this);
            return new SctExpressionStatementSyntax(node.Context, expression);
        }

        public override SctSyntax Visit(SctDeclarationStatementSyntax node)
        {
            var type = (SctTypeSyntax)node.Type.Accept(this);
            var expression = (SctExpressionSyntax)node.Expression.Accept(this);
            return new SctDeclarationStatementSyntax(node.Context, type, node.Id, expression);
        }

        public override SctSyntax Visit(SctAssignmentStatementSyntax node)
        {
            var expression = (SctExpressionSyntax)node.Expression.Accept(this);
            return new SctAssignmentStatementSyntax(node.Context, node.Id, expression);
        }

        public override SctSyntax Visit(SctIfStatementSyntax node)
        {
            var condition = (SctExpressionSyntax)node.Expression.Accept(this);
            var thenStatement = (SctBlockStatementSyntax)node.Then.Accept(this);
            var elseStatement = (SctElseStatementSyntax?)node.Else?.Accept(this);
            return new SctIfStatementSyntax(node.Context, condition, thenStatement, elseStatement);
        }

        public override SctSyntax Visit(SctElseStatementSyntax node)
        {
            var block = (SctBlockStatementSyntax)node.Block.Accept(this);
            return new SctElseStatementSyntax(node.Context, block);
        }

        public override SctSyntax Visit(SctWhileStatementSyntax node)
        {
            var condition = (SctExpressionSyntax)node.Expression.Accept(this);
            var body = (SctBlockStatementSyntax)node.Block.Accept(this);
            return new SctWhileStatementSyntax(node.Context, condition, body);
        }

        public override SctSyntax Visit(SctEnterStatementSyntax node)
        {
            return new SctEnterStatementSyntax(node.Context, node.Id);
        }

        public override SctSyntax Visit(SctExitStatementSyntax node)
        {
            return new SctExitStatementSyntax(node.Context);
        }

        public override SctSyntax Visit(SctReturnStatementSyntax node)
        {
            var expression = (SctExpressionSyntax?)node.Expression?.Accept(this);
            return new SctReturnStatementSyntax(node.Context, expression);
        }

        public override SctSyntax Visit(SctBreakStatementSyntax node)
        {
            return new SctBreakStatementSyntax(node.Context);
        }

        public override SctSyntax Visit(SctContinueStatementSyntax node)
        {
            return new SctContinueStatementSyntax(node.Context);
        }

        public override SctSyntax Visit(SctCreateStatementSyntax node)
        {
            var agent = (SctAgentExpressionSyntax)node.Agent.Accept(this);
            return new SctCreateStatementSyntax(node.Context, agent);
        }

        public override SctSyntax Visit(SctDestroyStatementSyntax node)
        {
            return new SctDestroyStatementSyntax(node.Context);
        }

        public override SctSyntax Visit(SctLiteralExpressionSyntax<long> node)
        {
            return new SctLiteralExpressionSyntax<long>(node.Context, node.Type, node.Value);
        }

        public override SctSyntax Visit(SctLiteralExpressionSyntax<double> node)
        {
            return new SctLiteralExpressionSyntax<double>(node.Context, node.Type, node.Value);
        }

        public override SctSyntax Visit(SctIdExpressionSyntax node)
        {
            return new SctIdExpressionSyntax(node.Context, node.Id);
        }

        public override SctSyntax Visit(SctParenthesisExpressionSyntax node)
        {
            var expression = (SctExpressionSyntax)node.Expression.Accept(this);
            return new SctParenthesisExpressionSyntax(node.Context, expression);
        }

        public override SctSyntax Visit(SctTypecastExpressionSyntax node)
        {
            var type = (SctTypeSyntax)node.Type.Accept(this);
            var expression = (SctExpressionSyntax)node.Expression.Accept(this);
            return new SctTypecastExpressionSyntax(node.Context, type, expression);
        }

        public override SctSyntax Visit(SctCallExpressionSyntax node)
        {
            var arguments = node.Expressions.Select(a => a.Accept(this)).Cast<SctExpressionSyntax>();
            return new SctCallExpressionSyntax(node.Context, node.Target, arguments);
        }

        public override SctSyntax Visit(SctUnaryMinusExpressionSyntax node)
        {
            var expression = (SctExpressionSyntax)node.Expression.Accept(this);
            return new SctUnaryMinusExpressionSyntax(node.Context, expression);
        }

        public override SctSyntax Visit(SctNotExpressionSyntax node)
        {
            var expression = (SctExpressionSyntax)node.Expression.Accept(this);
            return new SctNotExpressionSyntax(node.Context, expression);
        }

        public override SctSyntax Visit(SctBinaryExpressionSyntax node)
        {
            var left = (SctExpressionSyntax)node.Left.Accept(this);
            var right = (SctExpressionSyntax)node.Right.Accept(this);
            return new SctBinaryExpressionSyntax(node.Context, left, right, node.Op);
        }

        public override SctSyntax Visit(SctBooleanExpressionSyntax node)
        {
            var left = (SctExpressionSyntax)node.Left.Accept(this);
            var right = (SctExpressionSyntax)node.Right.Accept(this);
            return new SctBooleanExpressionSyntax(node.Context, left, right, node.Op);
        }

        public override SctSyntax Visit(SctAgentExpressionSyntax node)
        {
            var arguments = node.Fields.Select(a => a.Accept(this)).Cast<SctNamedArgumentSyntax>();
            return new SctAgentExpressionSyntax(node.Context, node.ClassName, node.StateName, arguments);
        }

        public override SctSyntax Visit(SctPredicateExpressionSyntax node)
        {
            var arguments = node.Fields.Select(a => a.Accept(this)).Cast<SctNamedArgumentSyntax>();
            return new SctPredicateExpressionSyntax(node.Context, node.ClassName, node.StateName, arguments);
        }

        public override SctSyntax Visit(SctNamedArgumentSyntax node)
        {
            var expression = (SctExpressionSyntax)node.Expression.Accept(this);
            return new SctNamedArgumentSyntax(node.Context, node.Id, expression);
        }

        public override SctSyntax Visit(SctTypeSyntax node)
        {
            return new SctTypeSyntax(node.Context, node.Type);
        }
    }
}

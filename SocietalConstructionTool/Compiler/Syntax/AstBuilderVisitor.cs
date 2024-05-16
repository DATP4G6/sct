using System.Globalization;

using Antlr4.Runtime.Misc;

namespace Sct.Compiler.Syntax
{
    public class AstBuilderVisitor : SctBaseVisitor<SctSyntax>
    {
        public override SctSyntax VisitStart([NotNull] SctParser.StartContext context)
        {
            var functions = context.function().Select(f => f.Accept(this)).Cast<SctFunctionSyntax>();
            var classes = context.class_def().Select(c => c.Accept(this)).Cast<SctClassSyntax>();
            return new SctProgramSyntax(context, functions, classes);
        }

        public override SctSyntax VisitFunction([NotNull] SctParser.FunctionContext context)
        {
            var name = context.ID().GetText();
            var body = (SctBlockStatementSyntax)context.statement_list().Accept(this);
            var type = (SctTypeSyntax)context.type().Accept(this);
            var parameters = ParseParameters(context.args_def());

            return new SctFunctionSyntax(context, name, parameters, type, body);
        }

        public override SctSyntax VisitClass_def([NotNull] SctParser.Class_defContext context)
        {
            var name = context.ID().GetText();
            var parameters = ParseParameters(context.args_def());

            var body = context.class_body();
            var states = body.state().Select(s => s.Accept(this)).Cast<SctStateSyntax>();
            var functions = body.function().Select(f => f.Accept(this)).Cast<SctFunctionSyntax>();
            var decorators = body.decorator().Select(d => d.Accept(this)).Cast<SctDecoratorSyntax>();
            return new SctClassSyntax(context, name, parameters, decorators, functions, states);
        }

        public override SctSyntax VisitState([NotNull] SctParser.StateContext context)
        {
            var name = context.ID().GetText();
            var decorators = context.state_decorator().Select(d => d.ID().GetText());
            var body = (SctBlockStatementSyntax)context.statement_list().Accept(this);
            return new SctStateSyntax(context, name, decorators, body);
        }

        public override SctSyntax VisitDecorator([NotNull] SctParser.DecoratorContext context)
        {
            var name = context.ID().GetText();
            var body = (SctBlockStatementSyntax)context.statement_list().Accept(this);
            return new SctDecoratorSyntax(context, name, body);
        }

        // Statements

        public override SctSyntax VisitStatement_list([NotNull] SctParser.Statement_listContext context)
        {
            var statements = context.statement().Select(s => s.Accept(this)).Cast<SctStatementSyntax>();
            return new SctBlockStatementSyntax(context, statements);
        }

        public override SctSyntax VisitExpressionStatement([NotNull] SctParser.ExpressionStatementContext context)
        {
            var expression = (SctExpressionSyntax)context.expression_statement().Accept(this);
            return new SctExpressionStatementSyntax(context, expression);
        }

        public override SctSyntax VisitVariableDeclaration([NotNull] SctParser.VariableDeclarationContext context)
        {
            var type = (SctTypeSyntax)context.type().Accept(this);
            var id = context.ID().GetText();
            var expression = (SctExpressionSyntax)context.expression().Accept(this);
            return new SctDeclarationStatementSyntax(context, type, id, expression);
        }

        public override SctSyntax VisitAssignment([NotNull] SctParser.AssignmentContext context)
        {
            var id = context.ID().GetText();
            var expression = (SctExpressionSyntax)context.expression().Accept(this);
            return new SctAssignmentStatementSyntax(context, id, expression);
        }

        public override SctSyntax VisitIf([NotNull] SctParser.IfContext context)
        {
            var condition = (SctExpressionSyntax)context.expression().Accept(this);
            var block = (SctBlockStatementSyntax)context.statement_list().Accept(this);
            // using `as` to allow for null values instead of explicit cast which throws
            var @else = context.@else()?.Accept(this) as SctElseStatementSyntax;
            @else ??= context.elseif()?.Accept(this) as SctElseStatementSyntax;
            return new SctIfStatementSyntax(context, condition, block, @else);
        }

        public override SctSyntax VisitElseif([NotNull] SctParser.ElseifContext context)
        {
            var condition = (SctExpressionSyntax)context.expression().Accept(this);
            var block = (SctBlockStatementSyntax)context.statement_list().Accept(this);
            // using `as` to allow for null values instead of explicit cast which throws
            var @else = context.@else()?.Accept(this) as SctElseStatementSyntax;
            @else ??= context.elseif()?.Accept(this) as SctElseStatementSyntax;
            var @if = new SctIfStatementSyntax(context, condition, block, @else);
            // else if is actually an else block with an if statement inside
            return new SctElseStatementSyntax(context, new SctBlockStatementSyntax(context, [@if]));
        }

        public override SctSyntax VisitElse([NotNull] SctParser.ElseContext context)
        {
            var block = (SctBlockStatementSyntax)context.statement_list().Accept(this);
            return new SctElseStatementSyntax(context, block);
        }

        public override SctSyntax VisitWhile([NotNull] SctParser.WhileContext context)
        {
            var condition = (SctExpressionSyntax)context.expression().Accept(this);
            var block = (SctBlockStatementSyntax)context.statement_list().Accept(this);
            return new SctWhileStatementSyntax(context, condition, block);
        }

        public override SctSyntax VisitEnter([NotNull] SctParser.EnterContext context)
        {
            var id = context.ID().GetText();
            return new SctEnterStatementSyntax(context, id);
        }

        public override SctSyntax VisitConditional_enter([NotNull] SctParser.Conditional_enterContext context)
        {
            var id = context.ID().GetText();
            var condition = (SctExpressionSyntax)context.expression().Accept(this);
            var enter = new SctEnterStatementSyntax(context, id);
            var block = new SctBlockStatementSyntax(context, [enter]);
            return new SctIfStatementSyntax(context, condition, block, null);
        }

        public override SctSyntax VisitExit([NotNull] SctParser.ExitContext context)
        {
            return new SctExitStatementSyntax(context);
        }

        public override SctSyntax VisitReturn([NotNull] SctParser.ReturnContext context)
        {
            var expression = (SctExpressionSyntax?)context.expression()?.Accept(this);
            return new SctReturnStatementSyntax(context, expression);
        }

        public override SctSyntax VisitBreak([NotNull] SctParser.BreakContext context)
        {
            return new SctBreakStatementSyntax(context);
        }

        public override SctSyntax VisitContinue([NotNull] SctParser.ContinueContext context)
        {
            return new SctContinueStatementSyntax(context);
        }

        public override SctSyntax VisitCreate([NotNull] SctParser.CreateContext context)
        {
            var agent = (SctAgentExpressionSyntax)context.agent_create().Accept(this);
            return new SctCreateStatementSyntax(context, agent);
        }

        public override SctSyntax VisitDestroy([NotNull] SctParser.DestroyContext context)
        {
            return new SctDestroyStatementSyntax(context);
        }

        // Expressions

        public override SctSyntax VisitLiteralExpression([NotNull] SctParser.LiteralExpressionContext context)
        {
            // Literals are handled a layer deeper, to include type information
            return context.literal().Accept(this);
        }

        public override SctSyntax VisitLiteral([NotNull] SctParser.LiteralContext context)
        {
            return context switch
            {
                { } when context.INT() != null => new SctLiteralExpressionSyntax<long>(context, SctType.Int, long.Parse(context.INT().GetText(), CultureInfo.InvariantCulture)),
                { } when context.FLOAT() != null => new SctLiteralExpressionSyntax<double>(context, SctType.Float, double.Parse(context.FLOAT().GetText(), CultureInfo.InvariantCulture)),
                _ => throw new InvalidOperationException("Literal was not an INT or FLOAT")
            };
        }

        public override SctSyntax VisitIDExpression([NotNull] SctParser.IDExpressionContext context)
        {
            var id = context.ID().GetText();
            return new SctIdExpressionSyntax(context, id);
        }

        public override SctSyntax VisitParenthesisExpression([NotNull] SctParser.ParenthesisExpressionContext context)
        {
            var expression = (SctExpressionSyntax)context.expression().Accept(this);
            return new SctParenthesisExpressionSyntax(context, expression);
        }

        public override SctSyntax VisitTypecastExpression([NotNull] SctParser.TypecastExpressionContext context)
        {
            var type = (SctTypeSyntax)context.type().Accept(this);
            var expression = (SctExpressionSyntax)context.expression().Accept(this);
            return new SctTypecastExpressionSyntax(context, type, expression);
        }

        public override SctSyntax VisitCallExpression([NotNull] SctParser.CallExpressionContext context)
        {
            var id = context.ID().GetText();
            var arguments = context.args_call().expression().Select(e => (SctExpressionSyntax)e.Accept(this));
            return new SctCallExpressionSyntax(context, id, arguments);
        }

        public override SctSyntax VisitUnaryMinusExpression([NotNull] SctParser.UnaryMinusExpressionContext context)
        {
            var expression = (SctExpressionSyntax)context.expression().Accept(this);
            return new SctUnaryMinusExpressionSyntax(context, expression);
        }

        public override SctSyntax VisitLogicalNotExpression([NotNull] SctParser.LogicalNotExpressionContext context)
        {
            var expression = (SctExpressionSyntax)context.expression().Accept(this);
            return new SctNotExpressionSyntax(context, expression);
        }

        public override SctSyntax VisitBinaryExpression([NotNull] SctParser.BinaryExpressionContext context)
        {
            var left = (SctExpressionSyntax)context.expression(0).Accept(this);
            var right = (SctExpressionSyntax)context.expression(1).Accept(this);
            var @operator = context.op.Type switch
            {
                SctLexer.PLUS => SctBinaryOperator.Plus,
                SctLexer.MINUS => SctBinaryOperator.Minus,
                SctLexer.MULT => SctBinaryOperator.Mult,
                SctLexer.DIV => SctBinaryOperator.Div,
                SctLexer.MOD => SctBinaryOperator.Mod,
                _ => throw new InvalidOperationException("Invalid binary operator")
            };
            return new SctBinaryExpressionSyntax(context, left, right, @operator);
        }

        public override SctSyntax VisitBooleanExpression([NotNull] SctParser.BooleanExpressionContext context)
        {
            var left = (SctExpressionSyntax)context.expression(0).Accept(this);
            var right = (SctExpressionSyntax)context.expression(1).Accept(this);
            var @operator = context.op.Type switch
            {
                SctLexer.AND => SctBooleanOperator.And,
                SctLexer.OR => SctBooleanOperator.Or,
                SctLexer.EQ => SctBooleanOperator.Eq,
                SctLexer.NEQ => SctBooleanOperator.Neq,
                SctLexer.LT => SctBooleanOperator.Lt,
                SctLexer.LTE => SctBooleanOperator.Lte,
                SctLexer.GT => SctBooleanOperator.Gt,
                SctLexer.GTE => SctBooleanOperator.Gte,
                _ => throw new InvalidOperationException("Invalid boolean operator")
            };
            return new SctBooleanExpressionSyntax(context, left, right, @operator);
        }

        public override SctSyntax VisitHashPredicateExpression([NotNull] SctParser.HashPredicateExpressionContext context)
        {
            var predicate = (SctExpressionSyntax)context.expression().Accept(this);
            return new SctCallExpressionSyntax(context, "count", [predicate]);
        }

        public override SctSyntax VisitAgent_create([NotNull] SctParser.Agent_createContext context)
        {
            var classId = context.ID(0).GetText();
            var stateId = context.ID(1).GetText();
            var fields = ParseArgsAgent(context.args_agent());
            return new SctAgentExpressionSyntax(context, classId, stateId, fields);
        }

        public override SctSyntax VisitAgent_predicate([NotNull] SctParser.Agent_predicateContext context)
        {
            var classId = context.ID(0).GetText();
            var stateId = context.QUESTION() != null ? null : context.ID(1).GetText();
            var fields = ParseArgsAgent(context.args_agent());
            return new SctPredicateExpressionSyntax(context, classId, stateId, fields);
        }

        public override SctSyntax VisitType([NotNull] SctParser.TypeContext context)
        {
            var type = context switch
            {
                { } when context.T_INT() != null => SctType.Int,
                { } when context.T_FLOAT() != null => SctType.Float,
                { } when context.T_PREDICATE() != null => SctType.Predicate,
                { } when context.T_VOID() != null => SctType.Void,
                _ => throw new InvalidOperationException("Invalid type")
            };
            return new SctTypeSyntax(context, type);
        }

        private IEnumerable<SctNamedArgumentSyntax> ParseArgsAgent(SctParser.Args_agentContext argsAgent)
        {
            if (argsAgent == null)
                return Enumerable.Empty<SctNamedArgumentSyntax>();
            var ids = argsAgent.ID().Select(id => id.GetText());
            var expressions = argsAgent.expression().Select(expression => expression.Accept(this)).Cast<SctExpressionSyntax>();
            var fields = ids.Zip(expressions,
                    (id, expression) => new SctNamedArgumentSyntax(argsAgent, id, expression));
            return fields;
        }

        private IEnumerable<SctParameterSyntax> ParseParameters(SctParser.Args_defContext context)
        {
            if (context == null)
                return Enumerable.Empty<SctParameterSyntax>();
            var ids = context.ID().Select(id => id.GetText());
            var types = context.type().Select(type => (SctTypeSyntax)type.Accept(this));
            return ids.Zip(types, (id, type) => new SctParameterSyntax(context, type, id));
        }
    }
}

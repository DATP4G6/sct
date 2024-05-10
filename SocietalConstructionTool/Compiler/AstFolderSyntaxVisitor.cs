using System.Numerics;

using Sct.Compiler.Syntax;

namespace Sct.Compiler
{
    public class AstFolderSyntaxVisitor : SctBaseBuilderSyntaxVisitor, IErrorReporter
    {
        public IEnumerable<CompilerError> Errors => _errors;
        private readonly List<CompilerError> _errors = [];

        public override SctSyntax Visit(SctBinaryExpressionSyntax node)
        {
            var left = (SctExpressionSyntax)node.Left.Accept(this);
            var right = (SctExpressionSyntax)node.Right.Accept(this);

            // Handle 0 division by catching expression
            try
            {
                return (left, right) switch
                {
                    // Both nodes are ints
                    (SctLiteralExpressionSyntax<long> intLeft, SctLiteralExpressionSyntax<long> intRight) =>
                        new SctLiteralExpressionSyntax<long>(
                            node.Context,
                            Syntax.SctType.Int,
                            Calculate(intLeft.Value, intRight.Value, node.Op)
                        ),
                    // One is int, the other is float
                    (SctLiteralExpressionSyntax<long> intLeft, SctLiteralExpressionSyntax<double> floatRight) =>
                        new SctLiteralExpressionSyntax<double>(
                            node.Context,
                            Syntax.SctType.Float,
                            Calculate(intLeft.Value, floatRight.Value, node.Op)
                        ),
                    // One is float, the other is int
                    (SctLiteralExpressionSyntax<double> floatLeft, SctLiteralExpressionSyntax<long> intRight) =>
                        new SctLiteralExpressionSyntax<double>(
                            node.Context,
                            Syntax.SctType.Float,
                            Calculate(floatLeft.Value, intRight.Value, node.Op)
                        ),
                    // Both nodes are floats
                    (SctLiteralExpressionSyntax<double> floatLeft, SctLiteralExpressionSyntax<double> floatRight) =>
                        new SctLiteralExpressionSyntax<double>(
                            node.Context,
                            Syntax.SctType.Float,
                            Calculate(floatLeft.Value, floatRight.Value, node.Op)
                        ),
                    _ => node
                };

            }
            catch (DivideByZeroException)
            {
                _errors.Add(new CompilerError("Division by zero", node.Context));
                return new SctBinaryExpressionSyntax(
                    node.Context.OriginalContext,
                    left,
                    right,
                    node.Op
                );
            }
        }

        public override SctSyntax Visit(SctBooleanExpressionSyntax node)
        {
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);

            return (left, right) switch
            {
                // Both nodes are ints
                (SctLiteralExpressionSyntax<long> intLeft, SctLiteralExpressionSyntax<long> intRight) =>
                    new SctLiteralExpressionSyntax<long>(
                        node.Context,
                        Syntax.SctType.Int,
                        Calculate(intLeft.Value, intRight.Value, node.Op)
                    ),
                // One is int, the other is float
                (SctLiteralExpressionSyntax<long> intLeft, SctLiteralExpressionSyntax<double> floatRight) =>
                    new SctLiteralExpressionSyntax<double>(
                        node.Context,
                        Syntax.SctType.Int,
                        Calculate(intLeft.Value, floatRight.Value, node.Op)
                    ),
                // One is float, the other is int
                (SctLiteralExpressionSyntax<double> floatLeft, SctLiteralExpressionSyntax<long> intRight) =>
                    new SctLiteralExpressionSyntax<double>(
                        node.Context,
                        Syntax.SctType.Int,
                        Calculate(floatLeft.Value, intRight.Value, node.Op)
                    ),
                // Both nodes are floats
                (SctLiteralExpressionSyntax<double> floatLeft, SctLiteralExpressionSyntax<double> floatRight) =>
                    new SctLiteralExpressionSyntax<double>(
                        node.Context,
                        Syntax.SctType.Int,
                        Calculate(floatLeft.Value, floatRight.Value, node.Op)
                    ),
                _ => node
            };
        }

        public override SctSyntax Visit(SctUnaryMinusExpressionSyntax node)
        {
            var expression = node.Expression.Accept(this);

            return expression switch
            {
                SctLiteralExpressionSyntax<long> intNode => new SctLiteralExpressionSyntax<long>(
                    node.Context,
                    Syntax.SctType.Int,
                    -intNode.Value
                ),
                SctLiteralExpressionSyntax<double> floatNode => new SctLiteralExpressionSyntax<double>(
                    node.Context,
                    Syntax.SctType.Float,
                    -floatNode.Value
                ),
                _ => node
            };
        }

        public override SctSyntax Visit(SctNotExpressionSyntax node)
        {
            var expression = node.Expression.Accept(this);

            return expression switch
            {
                SctLiteralExpressionSyntax<long> intNode => new SctLiteralExpressionSyntax<long>(
                    node.Context,
                    Syntax.SctType.Int,
                    intNode.Value == 0 ? 1 : 0
                ),
                SctLiteralExpressionSyntax<double> floatNode => new SctLiteralExpressionSyntax<long>(
                    node.Context,
                    Syntax.SctType.Int,
                    floatNode.Value == 0 ? 1 : 0
                ),
                _ => node
            };
        }

        public override SctSyntax Visit(SctParenthesisExpressionSyntax node)
        {
            var expression = node.Expression.Accept(this);

            return expression switch
            {
                SctLiteralExpressionSyntax<long> intNode => new SctLiteralExpressionSyntax<long>(
                    node.Context,
                    Syntax.SctType.Int,
                    intNode.Value
                ),
                SctLiteralExpressionSyntax<double> floatNode => new SctLiteralExpressionSyntax<double>(
                    node.Context,
                    Syntax.SctType.Float,
                    floatNode.Value
                ),
                _ => node
            };
        }

        public override SctSyntax Visit(SctTypecastExpressionSyntax node)
        {
            var expression = node.Expression.Accept(this);

            return expression switch
            {
                SctLiteralExpressionSyntax<long> intNode => node.Type.Type switch
                {
                    Syntax.SctType.Float => new SctLiteralExpressionSyntax<double>(
                        node.Context,
                        Syntax.SctType.Float,
                        intNode.Value
                    ),
                    _ => intNode
                },
                SctLiteralExpressionSyntax<double> floatNode => node.Type.Type switch
                {
                    Syntax.SctType.Int => new SctLiteralExpressionSyntax<long>(
                        node.Context,
                        Syntax.SctType.Int,
                        (long)floatNode.Value
                    ),
                    _ => floatNode
                },
                _ => node
            };
        }

        private static T Calculate<T>(T left, T right, SctBinaryOperator op) where T : INumber<T>
        {
            var value = op switch
            {
                SctBinaryOperator.Plus => left + right,
                SctBinaryOperator.Minus => left - right,
                SctBinaryOperator.Mult => left * right,
                SctBinaryOperator.Div when T.IsZero(right) => throw new DivideByZeroException(),
                SctBinaryOperator.Div => left / right,
                SctBinaryOperator.Mod => left % right,
                _ => throw new InvalidOperationException("Invalid operator")
            };

            return value;
        }

        private static int Calculate(double left, double right, SctBooleanOperator op)
        {
            var value = op switch
            {
                SctBooleanOperator.Eq => left == right,
                SctBooleanOperator.Neq => left != right,
                SctBooleanOperator.Lt => left < right,
                SctBooleanOperator.Lte => left <= right,
                SctBooleanOperator.Gt => left > right,
                SctBooleanOperator.Gte => left >= right,
                SctBooleanOperator.Or => left != 0 || right != 0,
                SctBooleanOperator.And => left != 0 && right != 0,
                _ => throw new InvalidOperationException("Invalid operator")
            };

            return value ? 1 : 0;
        }
    }
}

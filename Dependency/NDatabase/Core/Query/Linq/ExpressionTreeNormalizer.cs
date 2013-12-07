using System.Linq.Expressions;

namespace NDatabase.Core.Query.Linq
{
    internal sealed class ExpressionTreeNormalizer : ExpressionTransformer
    {
        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            return IsBooleanMemberAccess(lambda.Body)
                       ? Expression.Lambda(ExpandExpression(lambda.Body, true))
                       : base.VisitLambda(lambda);
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            if (u.NodeType != ExpressionType.Not)
                return base.VisitUnary(u);

            if (IsBooleanMemberAccess(u.Operand) || IsNonOptimizeableBooleanMethodCall(u.Operand))
                return ExpandExpression(u.Operand, false);

            return base.VisitUnary(u);
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            var expression = NormalizeBooleanMemberAccess(b);

            return base.VisitBinary(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression method)
        {
            Visit(method.Object);
            VisitExpressionList(method.Arguments);

            return IsNonOptimizeableBooleanMethodCall(method)
                       ? ExpandExpression(method, true)
                       : base.VisitMethodCall(method);
        }

        private static bool IsNonOptimizeableBooleanMethodCall(Expression expression)
        {
            return expression.NodeType == ExpressionType.Call
                   && expression.Type == typeof (bool)
                   && !IsOptimizeableMethodCall((MethodCallExpression) expression);
        }

        private static bool IsOptimizeableMethodCall(MethodCallExpression expression)
        {
            return OptimizeableMethodConstrains.CanBeOptimized(expression.Method);
        }

        private static BinaryExpression ExpandExpression(Expression expression, bool value)
        {
            return Expression.Equal(expression, Expression.Constant(value));
        }

        private static bool IsBooleanMemberAccess(Expression expression)
        {
            return expression.NodeType == ExpressionType.MemberAccess && expression.Type == typeof (bool);
        }

        private static BinaryExpression NormalizeBooleanMemberAccess(BinaryExpression expression)
        {
            if (!IsLogicalOperator(expression))
                return expression;

            if (IsBooleanMemberAccess(expression.Right))
                expression = Expression.MakeBinary(expression.NodeType, expression.Left,
                                                   ExpandExpression(expression.Right, true));

            if (IsBooleanMemberAccess(expression.Left))
                expression = Expression.MakeBinary(expression.NodeType, ExpandExpression(expression.Left, true),
                                                   expression.Right);

            return expression;
        }

        private static bool IsLogicalOperator(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.OrElse:
                case ExpressionType.AndAlso:
                    return true;

                default:
                    return false;
            }
        }

        public Expression Normalize(Expression expression)
        {
            return Visit(expression);
        }
    }
}
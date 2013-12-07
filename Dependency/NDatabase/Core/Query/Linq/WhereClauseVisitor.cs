using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using NDatabase.Api.Query;

namespace NDatabase.Core.Query.Linq
{
    internal sealed class WhereClauseVisitor : ExpressionQueryBuilder
    {
        private static readonly Dictionary<Expression, IQueryBuilderRecord> Cache =
            new Dictionary<Expression, IQueryBuilderRecord>();

        protected override Dictionary<Expression, IQueryBuilderRecord> GetCachingStrategy()
        {
            return Cache;
        }

        protected override void VisitMethodCall(MethodCallExpression m)
        {
            Visit(m.Object);
            VisitExpressionList(m.Arguments);

            if (OptimizeableMethodConstrains.IsStringMethod(m.Method))
            {
                ProcessStringMethod(m);
                return;
            }

            if (OptimizeableMethodConstrains.IsIListOrICollectionOfTMethod(m.Method))
            {
                ProcessCollectionMethod(m);
                return;
            }

            AnalyseMethod(Recorder, m.Method);
        }

        private void ProcessStringMethod(MethodCallExpression call)
        {
            switch (call.Method.Name)
            {
                case "EndsWith":
                {
                    var caseSensitive = IsCaseSensitive(call.Arguments);
                    RecordConstraintApplication(c => c.EndsWith(caseSensitive));
                    return;
                }
                case "StartsWith":
                {
                    var caseSensitive = IsCaseSensitive(call.Arguments);
                    RecordConstraintApplication(c => c.StartsWith(caseSensitive));
                    return;
                }

                case "Contains":
                    RecordConstraintApplication(c => c.Contains());
                    return;

                case "Equals":
                    RecordConstraintApplication(c => c.Equal());
                    return;
            }

            CannotConvertToSoda(call);
        }

        private static bool IsCaseSensitive(ReadOnlyCollection<Expression> arguments)
        {
            if (arguments.Count == 1)
                return true;

            var expression = arguments[1];

            if (expression.NodeType == ExpressionType.IsFalse)
                return true;

            if (expression.NodeType == ExpressionType.IsTrue)
                return false;

            if (expression.Type.IsEnum)
            {
                var constantExpression = expression as ConstantExpression;
                if (constantExpression != null)
                {
                    if (constantExpression.Value.ToString().EndsWith("IgnoreCase"))
                        return false;
                }

                return true;
            }

            return true;
        }

        private void RecordConstraintApplication(Func<IConstraint, IConstraint> application)
        {
            Recorder.Add(ctx => ctx.ApplyConstraint(application));
        }

        private void ProcessCollectionMethod(MethodCallExpression call)
        {
            switch (call.Method.Name)
            {
                case "Contains":
                    if (IsCallOnCollectionOfStrings(call))
                        RecordConstraintApplication(c => c.Contains());

                    return;
            }

            CannotConvertToSoda(call);
        }

        private static bool IsCallOnCollectionOfStrings(MethodCallExpression call)
        {
            return call.Method.DeclaringType.IsGenericType &&
                   call.Method.DeclaringType.GetGenericArguments()[0] == typeof (string);
        }

        private static bool IsComparisonExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsConditionalExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    return true;
                default:
                    return false;
            }
        }

        protected override void VisitBinary(BinaryExpression b)
        {
            if (IsConditionalExpression(b))
            {
                ProcessConditionalExpression(b);
                return;
            }

            if (IsComparisonExpression(b))
            {
                ProcessPredicateExpression(b);
                return;
            }

            CannotConvertToSoda(b);
        }

        protected override void VisitUnary(UnaryExpression u)
        {
            var operand = u.Operand;
            if (u.NodeType == ExpressionType.Not)
            {
                Visit(operand);
                RecordConstraintApplication(c => c.Not());
                return;
            }

            if (u.NodeType == ExpressionType.Convert)
            {
                Visit(operand);
                return;
            }

            CannotConvertToSoda(u);
        }

        private void ProcessConditionalExpression(BinaryExpression b)
        {
            VisitPreservingQuery(b.Left);
            VisitPreservingQuery(b.Right);

            switch (b.NodeType)
            {
                case ExpressionType.AndAlso:
                    Recorder.Add(ctx => ctx.ApplyConstraint(c => c.And(ctx.PopConstraint())));
                    break;
                case ExpressionType.OrElse:
                    Recorder.Add(ctx => ctx.ApplyConstraint(c => c.Or(ctx.PopConstraint())));
                    break;
            }
        }

        private void VisitPreservingQuery(Expression expression)
        {
            PreservingQuery(() => Visit(expression));
        }

        private void PreservingQuery(Action action)
        {
            Recorder.Add(ctx => ctx.SaveQuery());
            action();
            Recorder.Add(ctx => ctx.RestoreQuery());
        }

        private void ProcessPredicateExpression(BinaryExpression b)
        {
            if (ParameterReferenceOnLeftSide(b))
            {
                Visit(b.Left);
                Visit(b.Right);
            }
            else
            {
                Visit(b.Right);
                Visit(b.Left);
            }

            ProcessPredicateExpressionOperator(b);
        }

        protected override void VisitMemberAccess(MemberExpression m)
        {
            if (!StartsWithParameterReference(m)) 
                CannotConvertToSoda(m);

            ProcessMemberAccess(m);
        }

        protected override void VisitConstant(ConstantExpression c)
        {
            var value = c.Value;
            Recorder.Add(ctx => ctx.PushConstraint(ctx.CurrentQuery.Constrain(ctx.ResolveValue(value))));
        }

        static bool ParameterReferenceOnLeftSide(BinaryExpression b)
        {
            if (StartsWithParameterReference(b.Left)) 
                return true;

            if (StartsWithParameterReference(b.Right)) 
                return false;

            CannotConvertToSoda(b);
            return false;
        }

        private void ProcessPredicateExpressionOperator(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.Equal:
                    RecordConstraintApplication(c => c.Equal());
                    break;
                case ExpressionType.NotEqual:
                    RecordConstraintApplication(c => c.Equal().Not());
                    break;
                case ExpressionType.LessThan:
                    RecordConstraintApplication(c => c.Smaller());
                    break;
                case ExpressionType.LessThanOrEqual:
                    RecordConstraintApplication(c => c.Smaller().Equal());
                    break;
                case ExpressionType.GreaterThan:
                    RecordConstraintApplication(c => c.Greater());
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    RecordConstraintApplication(c => c.Greater().Equal());
                    break;
                default:
                    CannotConvertToSoda(b);
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NDatabase.Exceptions;
using NDatabase.Tool;

namespace NDatabase.Core.Query.Linq
{
    internal abstract class ExpressionQueryBuilder : ExpressionVisitor
    {
        protected QueryBuilderRecorder Recorder { get; private set; }

        public virtual IQueryBuilderRecord Process(LambdaExpression expression)
        {
            return ProcessExpression(SubtreeEvaluator.Evaluate(Normalize(expression)));
        }

        private static Expression Normalize(Expression expression)
        {
            return new ExpressionTreeNormalizer().Normalize(expression);
        }

        protected abstract Dictionary<Expression, IQueryBuilderRecord> GetCachingStrategy();

        private IQueryBuilderRecord ProcessExpression(Expression expression)
        {
            return GetCachingStrategy().GetOrAdd(expression, CreateRecord);
        }

        private IQueryBuilderRecord CreateRecord(Expression expression)
        {
            Recorder = new QueryBuilderRecorder();
            Visit(expression);
            return Recorder.Record;
        }

        private static bool IsParameter(Expression expression)
        {
            if (expression == null)
                return false;
            return expression.NodeType == ExpressionType.Parameter;
        }

        protected static bool StartsWithParameterReference(Expression expression)
        {
            if (IsParameter(expression))
                return true;

            var unary = expression as UnaryExpression;
            if (unary != null)
                return StartsWithParameterReference(unary.Operand);

            var me = expression as MemberExpression;
            if (me != null)
                return StartsWithParameterReference(me.Expression);

            var call = expression as MethodCallExpression;
            if (call != null && call.Object != null)
                return StartsWithParameterReference(call.Object);

            return false;
        }

        private static bool IsFieldAccessExpression(MemberExpression m)
        {
            return m.Member.MemberType == MemberTypes.Field;
        }

        private static bool IsPropertyAccessExpression(MemberExpression m)
        {
            return m.Member.MemberType == MemberTypes.Property;
        }

        protected static void AnalyseMethod(QueryBuilderRecorder recorder, MethodInfo method)
        {
            try
            {
                var analyser = new ReflectionMethodAnalyser(method);
                analyser.Run(recorder);
            }
            catch (Exception e)
            {
                throw new LinqQueryException(e.Message, e);
            }
        }

        private static MethodInfo GetGetMethod(MemberExpression m)
        {
            return ((PropertyInfo) m.Member).GetGetMethod();
        }

        protected void ProcessMemberAccess(MemberExpression m)
        {
            Visit(m.Expression);
            if (IsFieldAccessExpression(m))
            {
                var descendingEnumType = ResolveDescendingEnumType(m);
                Recorder.Add(
                    ctx =>
                        {
                            ctx.Descend(m.Member.Name);
                            ctx.PushDescendigFieldEnumType(descendingEnumType);
                        });

                return;
            }

            if (IsPropertyAccessExpression(m))
            {
                AnalyseMethod(Recorder, GetGetMethod(m));
                return;
            }

            CannotConvertToSoda(m);
        }

        private static Type ResolveDescendingEnumType(Expression expression)
        {
            return !expression.Type.IsEnum ? null : expression.Type;
        }

        protected static void CannotConvertToSoda(Expression e)
        {
            throw new LinqQueryException(e.ToString());
        }

        private static void CannotConvertToSoda(ElementInit init)
        {
            throw new LinqQueryException(init.ToString());
        }

        private static void CannotConvertToSoda(MemberBinding binding)
        {
            throw new LinqQueryException(binding.ToString());
        }

        protected override void VisitBinding(MemberBinding binding)
        {
            CannotConvertToSoda(binding);
        }

        protected override void VisitConditional(ConditionalExpression conditional)
        {
            CannotConvertToSoda(conditional);
        }

        protected override void VisitElementInitializer(ElementInit initializer)
        {
            CannotConvertToSoda(initializer);
        }

        protected override void VisitInvocation(InvocationExpression invocation)
        {
            CannotConvertToSoda(invocation);
        }

        protected override void VisitListInit(ListInitExpression init)
        {
            CannotConvertToSoda(init);
        }

        protected override void VisitNew(NewExpression nex)
        {
            CannotConvertToSoda(nex);
        }

        protected override void VisitNewArray(NewArrayExpression newArray)
        {
            CannotConvertToSoda(newArray);
        }
    }
}
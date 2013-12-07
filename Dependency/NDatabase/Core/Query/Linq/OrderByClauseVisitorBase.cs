using System.Linq.Expressions;
using NDatabase.Api.Query;

namespace NDatabase.Core.Query.Linq
{
    internal abstract class OrderByClauseVisitorBase : ExpressionQueryBuilder
    {
        protected abstract void ApplyDirection(IQuery query);

        protected override void VisitMethodCall(MethodCallExpression methodCall)
        {
            Visit(methodCall.Object);

            AnalyseMethod(Recorder, methodCall.Method);
        }

        protected override void VisitMemberAccess(MemberExpression m)
        {
            ProcessMemberAccess(m);
        }

        public override IQueryBuilderRecord Process(LambdaExpression expression)
        {
            if (!StartsWithParameterReference(expression.Body))
                CannotConvertToSoda(expression.Body);

            return ApplyDirection(base.Process(expression));
        }

        private IQueryBuilderRecord ApplyDirection(IQueryBuilderRecord record)
        {
            var recorder = new QueryBuilderRecorder(record);
            recorder.Add(ctx => ApplyDirection(ctx.CurrentQuery));
            return recorder.Record;
        }
    }
}
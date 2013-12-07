using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NDatabase.Core.Query.Linq
{
    internal sealed class SubtreeEvaluator : LinqQueryTranslator
    {
        private readonly HashSet<Expression> _candidates;

        private SubtreeEvaluator(HashSet<Expression> candidates)
        {
            _candidates = candidates;
        }

        public static Expression Evaluate(Expression expression)
        {
            var nominator = new Nominator(expression, exp => exp.NodeType != ExpressionType.Parameter);

            return new SubtreeEvaluator(nominator.Candidates).Visit(expression);
        }

        protected override Expression Visit(Expression expression)
        {
            if (expression == null) 
                return null;
            
            return _candidates.Contains(expression) 
                ? EvaluateCandidate(expression) 
                : base.Visit(expression);
        }

        private static Expression EvaluateCandidate(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
                return expression;

            var evaluator = Expression.Lambda(expression).Compile();
            return Expression.Constant(evaluator.DynamicInvoke(null), expression.Type);
        }

        #region Nested type: Nominator

        private sealed class Nominator : ExpressionTransformer
        {
            private readonly HashSet<Expression> _candidates = new HashSet<Expression>();
            private readonly Func<Expression, bool> _predicate;
            private bool _cannotBeEvaluated;

            public Nominator(Expression expression, Func<Expression, bool> predicate)
            {
                _predicate = predicate;

                Visit(expression);
            }

            public HashSet<Expression> Candidates
            {
                get { return _candidates; }
            }

            private void AddCandidate(Expression expression)
            {
                _candidates.Add(expression);
            }

            protected override Expression Visit(Expression expression)
            {
                if (expression == null)
                    return null;

                var saveCannotBeEvaluated = _cannotBeEvaluated;
                _cannotBeEvaluated = false;

                base.Visit(expression);

                if (_cannotBeEvaluated)
                    return expression;

                if (_predicate(expression))
                    AddCandidate(expression);
                else
                    _cannotBeEvaluated = true;

                _cannotBeEvaluated |= saveCannotBeEvaluated;

                return expression;
            }
        }

        #endregion
    }
}

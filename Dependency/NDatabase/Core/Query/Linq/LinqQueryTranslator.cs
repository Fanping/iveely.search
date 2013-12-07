using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using NDatabase.Api.Query;

namespace NDatabase.Core.Query.Linq
{
    internal class LinqQueryTranslator : ExpressionTransformer
    {
        private bool _optimize = true;

        private bool Optimize
        {
            get { return _optimize; }
            set { _optimize = value; }
        }

        public static Expression Translate(Expression expression)
        {
            return new LinqQueryTranslator().Visit(expression);
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            return lambda;
        }

        protected override Expression VisitMethodCall(MethodCallExpression call)
        {
            return IsQueryableExtensionMethod(call.Method)
                       ? ReplaceQueryableMethodCall(call)
                       : base.VisitMethodCall(call);
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            var queryable = constant.Value as ILinqQueryable;
            
            return queryable == null 
                ? constant 
                : Expression.Constant(queryable.GetQuery());
        }

        private static bool IsQueryableExtensionMethod(MethodInfo method)
        {
            return IsExtension(method)
                   && typeof (IQueryable).IsAssignableFrom(method.GetParameters().First().ParameterType);
        }

        private MethodCallExpression ReplaceQueryableMethodCall(MethodCallExpression call)
        {
            var target = null as Expression;
            if (call.Object != null) 
                target = Visit(call.Object);

            var arguments = VisitExpressionList(call.Arguments);
            var method = ReplaceQueryableMethod(call.Method);

            return Expression.Call(target, method, ProcessArguments(method, arguments));
        }

        private static IEnumerable<Expression> ProcessArguments(MethodInfo method, ReadOnlyCollection<Expression> arguments)
        {
            var parameters = method.GetParameters();

            return parameters.Select((t, i) => TryUnquoteExpression(arguments[i], t.ParameterType));
        }

        private static Expression TryUnquoteExpression(Expression expression, Type delegateType)
        {
            if (expression.NodeType != ExpressionType.Quote) 
                return expression;

            var lambda = (LambdaExpression)((UnaryExpression)expression).Operand;
            
            return lambda.Type == delegateType ? lambda : expression;
        }

        private MethodInfo ReplaceQueryableMethod(MethodInfo method)
        {
            MethodInfo match;

            if (Optimize)
            {
                if (TryMatchMethod(typeof (LinqQueryExtensions), method, out match))
                    return match;
            }

            if (TryMatchMethod(typeof (Enumerable), method, out match))
            {
                if (Optimize)
                    Optimize = false;
                return match;
            }

            throw new ArgumentException(string.Format("Failed to find a replacement for {0}", method));
        }

        private static bool TryMatchMethod(Type target, MethodInfo method, out MethodInfo match)
        {
            foreach (var candidate in target.GetMethods())
            {
                if (TryMatchMethod(method, candidate, out match)) 
                    return true;
            }

            match = null;
            return false;
        }

        private static bool TryMatchMethod(MethodInfo method, MethodInfo candidate, out MethodInfo match)
        {
            match = null;
            if (candidate.Name != method.Name) 
                return false;

            if (!IsExtension(candidate)) 
                return false;

            if (!LengthMatch(method.GetParameters(), candidate.GetParameters())) 
                return false;

            if (!TryMatchGenericMethod(method, ref candidate)) 
                return false;

            if (!TryMatchMethodSignature(method, candidate)) 
                return false;

            match = candidate;
            return true;
        }

        private static bool TryMatchGenericMethod(MethodBase method, ref MethodInfo candidate)
        {
            if (method.IsGenericMethod)
            {
                if (!candidate.IsGenericMethod) 
                    return false;
                if (!LengthMatch(candidate.GetGenericArguments(), method.GetGenericArguments())) 
                    return false;

                candidate = candidate.MakeGenericMethod(method.GetGenericArguments());
            }

            return true;
        }

        private static bool TryMatchMethodSignature(MethodInfo method, MethodInfo candidate)
        {
            var parameters = GetParameterTypes(method);
            var candidateParameters = GetParameterTypes(candidate);
            var compare = GetTypeComparer(candidate.DeclaringType);

            if (!compare(method.ReturnType, candidate.ReturnType)) 
                return false;

            for (var i = 0; i < candidateParameters.Length; i++)
            {
                if (!compare(parameters[i], candidateParameters[i])) 
                    return false;
            }

            return true;
        }

        private static bool LengthMatch<T1, T2>(ICollection<T1> a, ICollection<T2> b)
        {
            return a.Count == b.Count;
        }

        private static Func<Type, Type, bool> GetTypeComparer(Type type)
        {
            Func<Type, Type> mapper;
            if (!FuncMappers.TryGetValue(type, out mapper)) 
                mapper = t => t;

            return (a, b) => a == b || mapper(a) == b;
        }

        private static readonly Dictionary<Type, Func<Type, Type>> FuncMappers = new Dictionary<Type, Func<Type, Type>>
                                                                                     {
                                                                                         {
                                                                                             typeof (LinqQueryExtensions),
                                                                                             MapQueryableToNDb
                                                                                         },
                                                                                         {
                                                                                             typeof (Enumerable),
                                                                                             MapQueryableToEnumerable
                                                                                         },
                                                                                     };

        private static Type MapQueryableToNDb(Type type)
        {
            if (IsGenericInstanceOf(type, typeof (IQueryable<>)) ||
                IsGenericInstanceOf(type, typeof (IOrderedQueryable<>)))
            {
                type = typeof (ILinqQuery<>).MakeGenericType(type.GetGenericArguments());
            }

            return type;
        }

        private static Type MapQueryableToEnumerable(Type type)
        {
            if (IsGenericInstanceOf(type, typeof(IQueryable<>)))
            {
                type = typeof(IEnumerable<>).MakeGenericType(type.GetGenericArguments());
            }
            else if (IsGenericInstanceOf(type, typeof(IOrderedQueryable<>)))
            {
                type = typeof(IOrderedEnumerable<>).MakeGenericType(type.GetGenericArguments());
            }
            else if (IsGenericInstanceOf(type, typeof(Expression<>)))
            {
                type = GetFirstGenericArgument(type);
            }
            else if (type == typeof(IQueryable))
            {
                type = typeof(IEnumerable);
            }

            return type;
        }

        private static bool IsExtension(MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(typeof (ExtensionAttribute), false).Length > 0;
        }

        private static Type[] GetParameterTypes(MethodBase self)
        {
            return self.GetParameters().Select(p => p.ParameterType).ToArray();
        }

        private static bool IsGenericInstanceOf(Type enumerable, Type type)
        {
            return enumerable.IsGenericType && enumerable.GetGenericTypeDefinition() == type;
        }

        private static Type GetFirstGenericArgument(Type type)
        {
            return type.GetGenericArguments()[0];
        }
    }
}
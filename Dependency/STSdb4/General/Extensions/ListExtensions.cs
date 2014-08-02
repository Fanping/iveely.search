using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Iveely.STSdb4.General.Extensions
{
    public class ListHelper<T>
    {
        public static readonly ListHelper<T> Instance = new ListHelper<T>();

        public Action<List<T>, T[]> SetArray { get; private set; }
        public Func<List<T>, T[]> GetArray { get; private set; }
        public Action<List<T>, int> SetCount { get; private set; }
        public Action<List<T>> IncrementVersion { get; private set; }

        public Expression<Action<List<T>, T[]>> SetArrayLambda { get; private set; }
        public Expression<Func<List<T>, T[]>> GetArrayLambda { get; private set; }
        public Expression<Action<List<T>, int>> SetCountLambda { get; private set; }
        public Expression<Action<List<T>>> IncrementVersionLambda { get; private set; }

        public ListHelper()
        {
            SetArrayLambda = CreateSetArrayMethod();
            SetArray = SetArrayLambda.Compile();

            GetArrayLambda = CreateGetArrayMethod();
            GetArray = GetArrayLambda.Compile();

            SetCountLambda = CreateSetCountMethod();
            SetCount = SetCountLambda.Compile();

            IncrementVersionLambda = CreateIncremetVersionMethod();
            IncrementVersion = IncrementVersionLambda.Compile();
        }

        private Expression<Action<List<T>, T[]>> CreateSetArrayMethod()
        {
            var list = Expression.Parameter(typeof(List<T>), "list");
            var array = Expression.Parameter(typeof(T[]), "array");

            var assign = Expression.Assign(Expression.PropertyOrField(list, "_items"), array);

            return Expression.Lambda<Action<List<T>, T[]>>(assign, list, array);
        }

        private Expression<Func<List<T>, T[]>> CreateGetArrayMethod()
        {
            var list = Expression.Parameter(typeof(List<T>), "list");
            var items = Expression.PropertyOrField(list, "_items");

            return Expression.Lambda<Func<List<T>, T[]>>(Expression.Label(Expression.Label(typeof(T[])), items), list);
        }

        private Expression<Action<List<T>, int>> CreateSetCountMethod()
        {
            var list = Expression.Parameter(typeof(List<T>), "list");
            var count = Expression.Parameter(typeof(int), "count");

            var assign = Expression.Assign(Expression.PropertyOrField(list, "_size"), count);

            return Expression.Lambda<Action<List<T>, int>>(assign, list, count);
        }

        private Expression<Action<List<T>>> CreateIncremetVersionMethod()
        {
            var list = Expression.Parameter(typeof(List<T>), "list");

            var version = Expression.PropertyOrField(list, "_version");
            var assign = Expression.Assign(version, Expression.Add(version, Expression.Constant(1, typeof(int))));

            return Expression.Lambda<Action<List<T>>>(assign, list);
        }
    }

    public static class ListExtensions
    {
        public static T[] GetArray<T>(this List<T> instance)
        {
            return ListHelper<T>.Instance.GetArray(instance);
        }

        public static void SetArray<T>(this List<T> instance, T[] array)
        {
            ListHelper<T>.Instance.SetArray(instance, array);
        }

        public static void SetCount<T>(this List<T> instance, int count)
        {
            ListHelper<T>.Instance.SetCount(instance, count);
        }

        public static void IncrementVersion<T>(this List<T> instance)
        {
            ListHelper<T>.Instance.IncrementVersion(instance);
        }

        /// <summary>
        /// Splits the list into two parts, where the right part contains count elements and returns the right part of the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<T> Split<T>(this List<T> instance, int count)
        {
            if (instance.Count < count)
                throw new ArgumentException("list.Count < count");

            List<T> list = new List<T>(instance.Capacity);
            Array.Copy(instance.GetArray(), instance.Count - count, list.GetArray(), 0, count);
            list.SetCount(count);
            instance.SetCount(instance.Count - count);
            instance.IncrementVersion();

            return list;
        }

        public static void AddRange<T>(this List<T> instance, T[] array, int index, int count)
        {
            int newCount = instance.Count + count;

            if (instance.Capacity < newCount)
                instance.Capacity = newCount;

            Array.Copy(array, index, instance.GetArray(), instance.Count, count);
            instance.SetCount(newCount);
            instance.IncrementVersion();
        }

        public static void AddRange<T>(this List<T> instance, List<T> list, int index, int count)
        {
            instance.AddRange<T>(list.GetArray(), index, count);
        }
    }
}

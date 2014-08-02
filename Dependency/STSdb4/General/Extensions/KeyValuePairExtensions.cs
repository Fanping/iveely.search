using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Iveely.STSdb4.General.Extensions
{
    public delegate void SetKeyDelegate<TKey, TValue>(ref KeyValuePair<TKey, TValue> kv, TKey key);
    public delegate void SetValueDelegate<TKey, TValue>(ref KeyValuePair<TKey, TValue> kv, TValue value);
    public delegate void SetKeyValueDelegate<TKey, TValue>(ref KeyValuePair<TKey, TValue> kv, TKey key, TValue value);

    public class KeyValuePairHelper<TKey, TValue>
    {
        public static readonly KeyValuePairHelper<TKey, TValue> Instance = new KeyValuePairHelper<TKey, TValue>();

        public readonly SetKeyDelegate<TKey, TValue> SetKey;
        public readonly SetValueDelegate<TKey, TValue> SetValue;
        public readonly SetKeyValueDelegate<TKey, TValue> SetKeyValue;

        public readonly Expression<SetKeyDelegate<TKey, TValue>> SetKeyLambda;
        public readonly Expression<SetValueDelegate<TKey, TValue>> SetValueLambda;
        public readonly Expression<SetKeyValueDelegate<TKey, TValue>> SetKeyValueLambda;

        public KeyValuePairHelper()
        {
            SetKeyLambda = CreateSetKeyMethod();
            SetKey = SetKeyLambda.Compile();

            SetValueLambda = CreateSetValueMethod();
            SetValue = SetValueLambda.Compile();

            SetKeyValueLambda = CreateSetKeyValueMethod();
            SetKeyValue = SetKeyValueLambda.Compile();
        }

        private Expression<SetKeyDelegate<TKey, TValue>> CreateSetKeyMethod()
        {
            var kv = Expression.Parameter(typeof(KeyValuePair<TKey, TValue>).MakeByRefType(), "kv");
            var key = Expression.Parameter(typeof(TKey), "key");

            var assign = Expression.Assign(Expression.Field(kv, "key"), key);

            return Expression.Lambda<SetKeyDelegate<TKey, TValue>>(assign, kv, key);
        }

        private Expression<SetValueDelegate<TKey, TValue>> CreateSetValueMethod()
        {
            var kv = Expression.Parameter(typeof(KeyValuePair<TKey, TValue>).MakeByRefType(), "kv");
            var value = Expression.Parameter(typeof(TValue), "value");

            var assign = Expression.Assign(Expression.Field(kv, "value"), value);

            return Expression.Lambda<SetValueDelegate<TKey, TValue>>(assign, kv, value);
        }

        private Expression<SetKeyValueDelegate<TKey, TValue>> CreateSetKeyValueMethod()
        {
            var kv = Expression.Parameter(typeof(KeyValuePair<TKey, TValue>).MakeByRefType(), "kv");

            var key = Expression.Parameter(typeof(TKey), "key");
            var value = Expression.Parameter(typeof(TValue), "value");

            var body = Expression.Block(
                    Expression.Assign(Expression.Field(kv, "key"), key),
                    Expression.Assign(Expression.Field(kv, "value"), value)
                    );

            return Expression.Lambda<SetKeyValueDelegate<TKey, TValue>>(body, kv, key, value);
        }
    }
}
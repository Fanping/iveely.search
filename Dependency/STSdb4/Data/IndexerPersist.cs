using Iveely.General.Persist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Iveely.General.Extensions;
using System.Threading.Tasks;
using Iveely.General.Compression;

namespace Iveely.Data
{
    public class IndexerPersist<T> : IIndexerPersist<T>
    {
        public readonly Action<BinaryWriter, Func<int, T>, int> store;
        public readonly Action<BinaryReader, Action<int, T>, int> load;

        public IIndexerPersist[] Persists { get; private set; }
        public Type Type { get; private set; }
        public Func<Type, MemberInfo, int> MembersOrder { get; private set; }

        public Expression<Action<BinaryWriter, Func<int, T>, int>> LambdaStore { get; private set; }
        public Expression<Action<BinaryReader, Action<int, T>, int>> LambdaLoad { get; private set; }

        public IndexerPersist(IIndexerPersist[] persists, Func<Type, MemberInfo, int> membersOrder = null)
        {
            Type = typeof(T);
            MembersOrder = membersOrder;
            Persists = persists;

            LambdaStore = CreateStoreMethod();
            store = LambdaStore.Compile();

            LambdaLoad = CreateLoadMethod();
            load = LambdaLoad.Compile();
        }

        public IndexerPersist(Func<Type, MemberInfo, int> membersOrder = null)
            : this(IndexerPersistHelper.GetDefaultPersists(typeof(T), membersOrder), membersOrder)
        {
        }

        public Expression<Action<BinaryWriter, Func<int, T>, int>> CreateStoreMethod()
        {
            var writer = Expression.Parameter(typeof(BinaryWriter), "writer");
            var values = Expression.Parameter(typeof(Func<int, T>), "func");
            var count = Expression.Parameter(typeof(int), "count");

            var idx = Expression.Variable(typeof(int), "idx");
            var callValues = Expression.Call(values, values.Type.GetMethod("Invoke"), idx);

            var body = IndexerPersistHelper.CreateStoreBody(Type, Persists, writer, callValues, idx, count, MembersOrder);

            var lambda = Expression.Lambda<Action<BinaryWriter, Func<int, T>, int>>(body, new ParameterExpression[] { writer, values, count });

            return lambda;
        }

        public Expression<Action<BinaryReader, Action<int, T>, int>> CreateLoadMethod()
        {
            var reader = Expression.Parameter(typeof(BinaryReader), "reader");
            var values = Expression.Parameter(typeof(Action<int, T>), "func");
            var count = Expression.Parameter(typeof(int), "count");

            var array = Expression.Variable(typeof(T[]));


            var body = DataType.IsPrimitiveType(Type) ?
                    IndexerPersistHelper.SingleSlotCreateLoadBody(Type, false, values, reader, count, Persists) :
                    Expression.Block(new ParameterExpression[] { array },
                    Expression.Assign(array, Expression.New(array.Type.GetConstructor(new Type[] { typeof(int) }), count)),
                    array.For(i =>
                        {
                            return Expression.Block(Expression.Assign(Expression.ArrayAccess(array, i), Expression.New(typeof(T).GetConstructor(new Type[] { }))),
                                    Expression.Call(values, values.Type.GetMethod("Invoke"), i, Expression.ArrayAccess(array, i)));
                        }, Expression.Label(), count),
                    IndexerPersistHelper.CreateLoadBody(Type, false, reader, array, count, MembersOrder, Persists)
                    );

            return Expression.Lambda<Action<BinaryReader, Action<int, T>, int>>(body, new ParameterExpression[] { reader, values, count });
        }

        public void Store(BinaryWriter writer, Func<int, T> values, int count)
        {
            store(writer, values, count);
        }

        public void Load(BinaryReader reader, Action<int, T> values, int count)
        {
            load(reader, values, count);
        }

        #region Examples

        //public class Tick
        //{
        //    public string Symbol { get; set; }
        //    public DateTime Timestamp { get; set; }
        //    public double Bid { get; set; }
        //    public double Ask { get; set; }
        //    public long Volume { get; set; }
        //    public string Provider { get; set; }
        //}

        //public class TickIndexerPersist : IIndexerPersist<Tick>
        //{
        //    public Type Type { get; private set; }
        //    public IIndexerPersist[] Persists { get; private set; }

        //    public readonly Func<Type, MemberInfo, int> MembersOrder;

        //    public TickIndexerPersist(Type type, IIndexerPersist[] persist, Func<Type, MemberInfo, int> membersOrder = null)
        //    {
        //        Persists = persist;
        //        Type = type;
        //        MembersOrder = membersOrder;
        //    }

        //    public void Store(BinaryWriter writer, Func<int, Tick> values, int count)
        //    {
        //        Action[] actions = new Action[6];
        //        MemoryStream[] streams = new MemoryStream[6];

        //        actions[0] = () =>
        //        {
        //            streams[0] = new MemoryStream();
        //            ((StringIndexerPersist)Persists[0]).Store(new BinaryWriter(streams[0]), (idx) => values.Invoke(idx).Symbol, count);
        //        };

        //        actions[1] = () =>
        //        {
        //            streams[1] = new MemoryStream();
        //            ((DateTimeIndexerPersist)Persists[1]).Store(new BinaryWriter(streams[1]), (idx) => values.Invoke(idx).Timestamp, count);
        //        };

        //        actions[2] = () =>
        //        {
        //            streams[2] = new MemoryStream();
        //            ((DoubleIndexerPersist)Persists[2]).Store(new BinaryWriter(streams[2]), (idx) => values.Invoke(idx).Ask, count);
        //        };

        //        actions[3] = () =>
        //        {
        //            streams[3] = new MemoryStream();
        //            ((DoubleIndexerPersist)Persists[3]).Store(new BinaryWriter(streams[3]), (idx) => values.Invoke(idx).Bid, count);
        //        };

        //        actions[4] = () =>
        //        {
        //            streams[4] = new MemoryStream();
        //            ((Int64IndexerPersist)Persists[5]).Store(new BinaryWriter(streams[4]), (idx) => values.Invoke(idx).Volume, count);
        //        };

        //        actions[5] = () =>
        //        {
        //            streams[5] = new MemoryStream();
        //            ((StringIndexerPersist)Persists[5]).Store(new BinaryWriter(streams[5]), (idx) => values.Invoke(idx).Provider, count);
        //        };

        //        Parallel.Invoke(actions);

        //        for (int i = 0; i < actions.Length; i++)
        //        {
        //            var stream = streams[i];
        //            using (stream)
        //            {
        //                CountCompression.Serialize(writer, (ulong)stream.Length);
        //                writer.Write(stream.GetBuffer(), 0, (int)stream.Length);
        //            }
        //        }
        //    }

        //    public void Load(BinaryReader reader, Action<int, Tick> values, int count)
        //    {
        //        Tick[] array = new Tick[count];
        //        for (int i = 0; i < count; i++)
        //        {
        //            var item = new Tick();
        //            array[i] = item;
        //            values(i, item);
        //        }

        //        Action[] actions = new Action[6];
        //        byte[][] buffers = new byte[6][];

        //        for (int i = 0; i < 6; i++)
        //            buffers[i] = reader.ReadBytes((int)CountCompression.Deserialize(reader));

        //        actions[0] = () =>
        //        {
        //            using (MemoryStream ms = new MemoryStream(buffers[0]))
        //                ((IIndexerPersist<String>)Persists[0]).Load(new BinaryReader(ms), (idx, value) => { array[idx].Symbol = value; }, count);
        //        };

        //        actions[1] = () =>
        //        {
        //            using (MemoryStream ms = new MemoryStream(buffers[1]))
        //                ((IIndexerPersist<DateTime>)Persists[1]).Load(new BinaryReader(ms), (idx, value) => { array[idx].Timestamp = value; }, count);
        //        };

        //        actions[2] = () =>
        //        {
        //            using (MemoryStream ms = new MemoryStream(buffers[2]))
        //                ((IIndexerPersist<Double>)Persists[2]).Load(new BinaryReader(ms), (idx, value) => { array[idx].Bid = value; }, count);
        //        };

        //        actions[3] = () =>
        //        {
        //            using (MemoryStream ms = new MemoryStream(buffers[3]))
        //                ((IIndexerPersist<Double>)Persists[3]).Load(new BinaryReader(ms), (idx, value) => { array[idx].Ask = value; }, count);
        //        };

        //        actions[4] = () =>
        //        {
        //            using (MemoryStream ms = new MemoryStream(buffers[4]))
        //                ((IIndexerPersist<Int64>)Persists[4]).Load(new BinaryReader(ms), (idx, value) => { array[idx].Volume = value; }, count);
        //        };

        //        actions[5] = () =>
        //        {
        //            using (MemoryStream ms = new MemoryStream(buffers[5]))
        //                ((IIndexerPersist<String>)Persists[5]).Load(new BinaryReader(ms), (idx, value) => { array[idx].Provider = value; }, count);
        //        };

        //        Parallel.Invoke(actions);
        //    }
        //}

        #endregion
    }

    internal static class IndexerPersistHelper
    {
        internal static Expression CreateLoadBody(Type type, bool isData, Expression reader, ParameterExpression array, ParameterExpression count, Func<Type, MemberInfo, int> membersOrder, IIndexerPersist[] persists)
        {
            int countOfType = type.GetPublicReadWritePropertiesAndFields().Count();

            List<Expression> list = new List<Expression>();

            var actionsArray = Expression.Variable(typeof(Action[]));
            list.Add(Expression.Assign(actionsArray, Expression.New(actionsArray.Type.GetConstructor(new Type[] { typeof(int) }), Expression.Constant(countOfType))));

            var buffers = Expression.Variable(typeof(byte[][]));
            list.Add(Expression.Assign(buffers, Expression.New(buffers.Type.GetConstructor(new Type[] { typeof(int) }), Expression.Constant(countOfType))));

            list.Add(buffers.For(i =>
                Expression.Assign(Expression.ArrayAccess(buffers, i),
                    Expression.Call(reader, typeof(BinaryReader).GetMethod("ReadBytes"),
                        Expression.Convert(Expression.Call(typeof(CountCompression).GetMethod("Deserialize"), reader), typeof(int)))), Expression.Label(), Expression.Constant(countOfType))
                    );

            var ms = Expression.Variable(typeof(MemoryStream));

            int j = 0;
            foreach (var member in type.GetPublicReadWritePropertiesAndFields())
                list.Add(Expression.Assign(Expression.ArrayAccess(actionsArray, Expression.Constant(j)),
                                           Expression.Lambda(ms.Using(Expression.New(typeof(MemoryStream).GetConstructor(new Type[] { typeof(byte[]) }),
                                            Expression.ArrayAccess(buffers, Expression.Constant(j))), GetLoadPersistCall(j++, isData, member, array, ms, count, persists))))
                        );

            list.Add(Expression.Call(typeof(Parallel).GetMethod("Invoke", new Type[] { typeof(Action[]) }), actionsArray));

            return Expression.Block(new ParameterExpression[] { actionsArray, buffers, ms }, list);
        }

        private static Expression GetLoadPersistCall(int index, bool isData, MemberInfo member, ParameterExpression array, ParameterExpression ms, ParameterExpression count, IIndexerPersist[] persists)
        {
            //  ((IIndexerPersist<string>)persist[0]).Load(new BinaryReader(ms), (idx, value) => { array[idx].Slot0 = value; }, count);

            var idx = Expression.Variable(typeof(int), "idx");
            var value = Expression.Variable(member.GetPropertyOrFieldType(), "value");

            var field = isData ? Expression.PropertyOrField(Expression.ArrayAccess(array, idx).Value(), member.Name) : Expression.PropertyOrField(Expression.ArrayAccess(array, idx), member.Name);

            return Expression.Call(Expression.Convert(Expression.Constant(persists[index]), persists[index].GetType()), persists[index].GetType().GetMethod("Load"),
               Expression.New(typeof(BinaryReader).GetConstructor(new Type[] { typeof(MemoryStream) }), ms),
               Expression.Lambda(typeof(Action<,>).MakeGenericType(new Type[] { typeof(int), member.GetPropertyOrFieldType() }), Expression.Assign(field, value), idx, value),
               count);
        }

        internal static Expression SingleSlotCreateLoadBody(Type type, bool isData, Expression values, Expression reader, ParameterExpression count, IIndexerPersist[] persists)
        {
            List<Expression> list = new List<Expression>();
            var buffers = Expression.Variable(typeof(byte[]));
            list.Add(Expression.Assign(buffers, Expression.Call(reader, typeof(BinaryReader).GetMethod("ReadBytes"), Expression.Convert(Expression.Call(typeof(CountCompression).GetMethod("Deserialize"), reader), typeof(int)))));

            var ms = Expression.Variable(typeof(MemoryStream));
            var idx = Expression.Variable(typeof(int), "idx");
            var value = Expression.Variable(type, "value");

            list.Add(ms.Using(Expression.New(typeof(MemoryStream).GetConstructor(new Type[] { typeof(byte[]) }), buffers),
                   Expression.Call(Expression.Convert(Expression.Constant(persists[0]),
                       persists[0].GetType()), persists[0].GetType().GetMethod("Load"),
                       Expression.New(typeof(BinaryReader).GetConstructor(new Type[] { typeof(MemoryStream) }), ms),
                       Expression.Lambda(isData ?
                           Expression.Call(values, values.Type.GetMethod("Invoke"), idx, Expression.New(typeof(Data<>).MakeGenericType(type).GetConstructor(new Type[] { type }), value))
                           : Expression.Call(values, values.Type.GetMethod("Invoke"), idx, value),
                           idx, value),
                       count)));

            return Expression.Block(new ParameterExpression[] { buffers }, list);
        }

        internal static Expression CreateStoreBody(Type type, IIndexerPersist[] persists, Expression writer, Expression callValues, ParameterExpression idx, Expression count, Func<Type, MemberInfo, int> membersOrder)
        {
            List<Expression> list = new List<Expression>();
            int itemsCount = DataTypeUtils.GetPublicMembers(type, membersOrder).Count();

            //Create body for single item.
            if (itemsCount <= 1)
            {
                var persist = Expression.Convert(Expression.Constant(persists[0]), persists[0].GetType());
                var ms = Expression.Variable(typeof(MemoryStream), "ms");
                var func = Expression.Lambda(itemsCount == 0 ? callValues : Expression.PropertyOrField(callValues, DataTypeUtils.GetPublicMembers(type, membersOrder).First().Name), idx);

                return ms.Using(Expression.Block(
                        Expression.Assign(ms, Expression.New(typeof(MemoryStream).GetConstructor(new Type[] { }))),
                        Expression.Call(persist, persist.Type.GetMethod("Store"), Expression.New(typeof(BinaryWriter).GetConstructor(new Type[] { typeof(MemoryStream) }), ms), func, count),
                        Expression.Call(typeof(CountCompression).GetMethod("Serialize"), writer, Expression.ConvertChecked(Expression.Property(ms, "Length"), typeof(ulong))),
                        Expression.Call(writer, typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(byte[]), typeof(int), typeof(int) }),
                            Expression.Call(ms, typeof(MemoryStream).GetMethod("GetBuffer")), Expression.Constant(0), Expression.Convert(Expression.Property(ms, "Length"), typeof(int)))
                    ));
            }
            else
            {
                var streams = Expression.Variable(typeof(MemoryStream[]), "streams");
                var actions = Expression.Variable(typeof(Action[]), "actions");

                list.Add(Expression.Assign(streams, Expression.New(typeof(MemoryStream[]).GetConstructor(new Type[] { typeof(int) }), Expression.Constant(itemsCount, typeof(int)))));
                list.Add(Expression.Assign(actions, Expression.New(typeof(Action[]).GetConstructor(new Type[] { typeof(int) }), Expression.Constant(itemsCount, typeof(int)))));

                int counter = 0;
                foreach (var member in DataTypeUtils.GetPublicMembers(type, membersOrder))
                {
                    var persist = Expression.Convert(Expression.Constant(persists[counter]), persists[counter].GetType());
                    var ms = Expression.ArrayAccess(streams, Expression.Constant(counter, typeof(int)));
                    var func = Expression.Lambda(Expression.PropertyOrField(callValues, member.Name), idx);

                    var writerNew = Expression.New(typeof(BinaryWriter).GetConstructor(new Type[] { typeof(MemoryStream) }), ms);

                    var action = Expression.Lambda(Expression.Block(
                           Expression.Assign(ms, Expression.New(typeof(MemoryStream).GetConstructor(new Type[] { }))),
                           Expression.Call(persist, persist.Type.GetMethod("Store"), writerNew, func, count)
                        ));

                    list.Add(Expression.Assign(Expression.ArrayAccess(actions, Expression.Constant(counter)), action));
                    counter++;
                }

                list.Add(Expression.Call(typeof(Parallel).GetMethod("Invoke", new Type[] { typeof(Action[]) }), actions));

                list.Add(streams.For(
                    (i) =>
                    {
                        var stream = Expression.Variable(typeof(MemoryStream), "stream");

                        return stream.Using(Expression.Block(
                               Expression.Assign(stream, Expression.ArrayAccess(streams, i)),
                               Expression.Call(typeof(CountCompression).GetMethod("Serialize"), writer, Expression.ConvertChecked(Expression.Property(stream, "Length"), typeof(ulong))),
                               Expression.Call(writer, typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(byte[]), typeof(int), typeof(int) }),
                                   Expression.Call(stream, typeof(MemoryStream).GetMethod("GetBuffer")), Expression.Constant(0), Expression.Convert(Expression.Property(stream, "Length"), typeof(int)))
                            ));
                    },
                     Expression.Label()
                ));

                return Expression.Block(new ParameterExpression[] { actions, streams }, list);
            }
        }

        public static IIndexerPersist[] GetDefaultPersists(Type type, Func<Type, MemberInfo, int> membersOrder = null)
        {
            if (DataType.IsPrimitiveType(type))
                return new IIndexerPersist[] { GetDefaultPersist(type) };

            List<IIndexerPersist> list = new List<IIndexerPersist>();
            foreach (var member in type.GetPublicReadWritePropertiesAndFields())
                list.Add(GetDefaultPersist(member.GetPropertyOrFieldType()));

            return list.ToArray();
        }

        private static IIndexerPersist GetDefaultPersist(Type type)
        {
            if (type == typeof(bool))
                return new BooleanIndexerPersist();
            if (type == typeof(char))
                return new CharIndexerPersist();
            if (type == typeof(byte))
                return new ByteIndexerPersist();
            if (type == typeof(sbyte))
                return new SByteIndexerPersist();
            if (type == typeof(Int16))
                return new Int16IndexerPersist();
            if (type == typeof(UInt16))
                return new UInt16IndexerPersist();
            if (type == typeof(Int32))
                return new Int32IndexerPersist();
            if (type == typeof(UInt32))
                return new UInt32IndexerPersist();
            if (type == typeof(Int64))
                return new Int64IndexerPersist();
            if (type == typeof(UInt64))
                return new UInt64IndexerPersist();
            if (type == typeof(Single))
                return new SingleIndexerPersist();
            if (type == typeof(Double))
                return new DoubleIndexerPersist();
            if (type == typeof(Decimal))
                return new DecimalIndexerPersist();
            if (type == typeof(String))
                return new StringIndexerPersist();
            if (type == typeof(DateTime))
                return new DateTimeIndexerPersist();
            if (type == typeof(byte[]))
                return new ByteArrayIndexerPersist();

            throw new NotSupportedException(type.ToString());
        }
    }
}
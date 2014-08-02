using Iveely.STSdb4.General.Persist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Iveely.STSdb4.General.Extensions;
using System.Threading.Tasks;
using Iveely.STSdb4.General.Compression;

namespace Iveely.STSdb4.Data
{
    public class DataIndexerPersist : IIndexerPersist<IData>
    {
        public readonly Action<BinaryWriter, Func<int, IData>, int> store;
        public readonly Action<BinaryReader, Action<int, IData>, int> load;

        public IIndexerPersist[] Persists { get; private set; }
        public Type Type { get; private set; }

        public Func<Type, MemberInfo, int> MembersOrder { get; private set; }

        public Expression<Action<BinaryWriter, Func<int, IData>, int>> LambdaStore { get; private set; }
        public Expression<Action<BinaryReader, Action<int, IData>, int>> LambdaLoad { get; private set; }

        public DataIndexerPersist(Type type, IIndexerPersist[] persists, Func<Type, MemberInfo, int> membersOrder = null)
        {
            Persists = persists;
            Type = type;

            MembersOrder = membersOrder;

            LambdaStore = CreateStoreMethod();
            store = LambdaStore.Compile();

            LambdaLoad = CreateLoadMethod();
            load = LambdaLoad.Compile();
        }

        public DataIndexerPersist(Type T, Func<Type, MemberInfo, int> membersOrder = null)
            : this(T, IndexerPersistHelper.GetDefaultPersists(T, membersOrder), membersOrder)
        {
        }

        private Expression<Action<BinaryWriter, Func<int, IData>, int>> CreateStoreMethod()
        {
            var writer = Expression.Parameter(typeof(BinaryWriter), "writer");
            var values = Expression.Parameter(typeof(Func<int, IData>), "values");
            var count = Expression.Parameter(typeof(int), "count");

            var idx = Expression.Variable(typeof(int), "idx");
            var callValues = Expression.Convert(Expression.Call(values, values.Type.GetMethod("Invoke"), idx), typeof(Data<>).MakeGenericType(Type)).Value();

            var body = IndexerPersistHelper.CreateStoreBody(Type, Persists, writer, callValues, idx, count, MembersOrder);
            var lambda = Expression.Lambda<Action<BinaryWriter, Func<int, IData>, int>>(body, new ParameterExpression[] { writer, values, count });

            return lambda;
        }

        public Expression<Action<BinaryReader, Action<int, IData>, int>> CreateLoadMethod()
        {
            var reader = Expression.Parameter(typeof(BinaryReader), "reader");
            var values = Expression.Parameter(typeof(Action<int, IData>), "func");
            var count = Expression.Parameter(typeof(int), "count");

            var array = Expression.Variable(typeof(Data<>).MakeGenericType(Type).MakeArrayType());

            var body = DataType.IsPrimitiveType(Type) ?
                    IndexerPersistHelper.SingleSlotCreateLoadBody(Type, true, values, reader, count, Persists) :
                    Expression.Block(new ParameterExpression[] { array },
                        Expression.Assign(array, Expression.New(array.Type.GetConstructor(new Type[] { typeof(int) }), count)),
                        array.For(i =>
                        {
                            return Expression.Block(Expression.Assign(Expression.ArrayAccess(array, i), Expression.New(typeof(Data<>).MakeGenericType(Type).GetConstructor(new Type[] { }))),
                                  Expression.Assign(Expression.ArrayAccess(array, i).Value(), Expression.New(Type.GetConstructor(new Type[] { }))),
                                    Expression.Call(values, values.Type.GetMethod("Invoke"), i, Expression.ArrayAccess(array, i)));
                        }, Expression.Label(), count),
                        IndexerPersistHelper.CreateLoadBody(Type, true, reader, array, count, MembersOrder, Persists)
                    );

            return Expression.Lambda<Action<BinaryReader, Action<int, IData>, int>>(body, new ParameterExpression[] { reader, values, count });
        }

        public void Store(BinaryWriter writer, Func<int, IData> values, int count)
        {
            store(writer, values, count);
        }

        public void Load(BinaryReader reader, Action<int, IData> values, int count)
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

        //public class TickIndexerPersist : IIndexerPersist<IData>
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

        //    public void Store(BinaryWriter writer, Func<int, IData> values, int count)
        //    {
        //        Action[] actions = new Action[6];
        //        MemoryStream[] streams = new MemoryStream[6];

        //        actions[0] = () =>
        //        {
        //            streams[0] = new MemoryStream();
        //            ((StringIndexerPersist)Persists[0]).Store(new BinaryWriter(streams[0]), (idx) => ((Data2<Tick>)values.Invoke(idx)).Value.Symbol, count);
        //        };

        //        actions[1] = () =>
        //        {
        //            streams[1] = new MemoryStream();
        //            ((DateTimeIndexerPersist)Persists[1]).Store(new BinaryWriter(streams[1]), (idx) => ((Data2<Tick>)values.Invoke(idx)).Value.Timestamp, count);
        //        };

        //        actions[2] = () =>
        //        {
        //            streams[2] = new MemoryStream();
        //            ((DoubleIndexerPersist)Persists[2]).Store(new BinaryWriter(streams[2]), (idx) => ((Data2<Tick>)values.Invoke(idx)).Value.Ask, count);
        //        };

        //        actions[3] = () =>
        //        {
        //            streams[3] = new MemoryStream();
        //            ((DoubleIndexerPersist)Persists[3]).Store(new BinaryWriter(streams[3]), (idx) => ((Data2<Tick>)values.Invoke(idx)).Value.Bid, count);
        //        };

        //        actions[4] = () =>
        //        {
        //            streams[4] = new MemoryStream();
        //            ((Int64IndexerPersist)Persists[4]).Store(new BinaryWriter(streams[4]), (idx) => ((Data2<Tick>)values.Invoke(idx)).Value.Volume, count);
        //        };

        //        actions[5] = () =>
        //        {
        //            streams[5] = new MemoryStream();
        //            ((StringIndexerPersist)Persists[5]).Store(new BinaryWriter(streams[5]), (idx) => ((Data2<Tick>)values.Invoke(idx)).Value.Provider, count);
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

        //    public void Load(BinaryReader reader, Action<int, IData> values, int count)
        //    {
        //        Data2<Tick>[] array = new Data2<Tick>[count];
        //        for (int i = 0; i < count; i++)
        //        {
        //            var item = new Data2<Tick>();
        //            item.Value = new Tick();
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
        //                ((IIndexerPersist<String>)Persists[0]).Load(new BinaryReader(ms), (idx, value) => { ((Data2<Tick>)array[idx]).Value.Symbol = value; }, count);
        //        };

        //        actions[1] = () =>
        //        {
        //            using (MemoryStream ms = new MemoryStream(buffers[1]))
        //                ((IIndexerPersist<DateTime>)Persists[1]).Load(new BinaryReader(ms), (idx, value) => { ((Data2<Tick>)array[idx]).Value.Timestamp = value; }, count);
        //        };

        //        actions[2] = () =>
        //        {
        //            using (MemoryStream ms = new MemoryStream(buffers[2]))
        //                ((IIndexerPersist<Double>)Persists[2]).Load(new BinaryReader(ms), (idx, value) => { ((Data2<Tick>)array[idx]).Value.Bid = value; }, count);
        //        };

        //        actions[3] = () =>
        //        {
        //            using (MemoryStream ms = new MemoryStream(buffers[3]))
        //                ((IIndexerPersist<Double>)Persists[3]).Load(new BinaryReader(ms), (idx, value) => { ((Data2<Tick>)array[idx]).Value.Ask = value; }, count);
        //        };

        //        actions[4] = () =>
        //        {
        //            using (MemoryStream ms = new MemoryStream(buffers[4]))
        //                ((IIndexerPersist<Int64>)Persists[4]).Load(new BinaryReader(ms), (idx, value) => { ((Data2<Tick>)array[idx]).Value.Volume = value; }, count);
        //        };

        //        actions[5] = () =>
        //        {
        //            using (MemoryStream ms = new MemoryStream(buffers[5]))
        //                ((IIndexerPersist<String>)Persists[5]).Load(new BinaryReader(ms), (idx, value) => { ((Data2<Tick>)array[idx]).Value.Provider = value; }, count);
        //        };

        //        Parallel.Invoke(actions);
        //    }
        //}
        #endregion

    }
}

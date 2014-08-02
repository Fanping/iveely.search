using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Iveely.STSdb4.Data
{
    public class DataTransformer<T> : ITransformer<T, IData>
    {
        public readonly Func<T, IData> to;
        public readonly Func<IData, T> from;

        public Type Type1 { get; private set; }
        public Type Type2 { get; private set; }

        public Func<Type, MemberInfo, int> MembersOrder1 { get; private set; }
        public Func<Type, MemberInfo, int> MembersOrder2 { get; private set; }

        public Expression<Func<T, IData>> LambdaTo { get; private set; }
        public Expression<Func<IData, T>> LambdaFrom { get; private set; }

        public DataTransformer(Type type2, Func<Type, MemberInfo, int> membersOrder1 = null, Func<Type, MemberInfo, int> membersOrder2 = null)
        {
            if (!TransformerHelper.CheckCompatible(typeof(T), type2, new HashSet<Type>(), membersOrder1, membersOrder2))
                throw new ArgumentException(String.Format("Type {0} is not compatible with {1}", typeof(T), type2));

            Type1 = typeof(T);
            Type2 = type2;

            MembersOrder1 = membersOrder1;
            MembersOrder2 = membersOrder2;

            LambdaTo = CreateToMethod();
            to = LambdaTo.Compile();

            LambdaFrom = CreateFromMethod();
            from = LambdaFrom.Compile(); 
        }

        private Expression<Func<T, IData>> CreateToMethod()
        {
            var value = Expression.Parameter(Type1);
            var data = Expression.Variable(typeof(Data<>).MakeGenericType(Type2));

            List<Expression> list = new List<Expression>();
            if (TransformerHelper.IsEqualsTypes(Type1, Type2))
                list.Add(Expression.Label(Expression.Label(typeof(IData)), Expression.New(data.Type.GetConstructor(new Type[] { Type1 }), value)));
            else
            {
                list.Add(Expression.Assign(data, Expression.New(data.Type.GetConstructor(new Type[] { }))));
                list.Add(TransformerHelper.BuildBody(data.Value(), value, MembersOrder1, MembersOrder2));
                list.Add(Expression.Label(Expression.Label(data.Type), data));
            }

            return Expression.Lambda<Func<T, IData>>(TransformerHelper.IsEqualsTypes(Type1, Type2) ? list[0] : Expression.Block(typeof(IData), new ParameterExpression[] { data }, list), value);
        }

        private Expression<Func<IData, T>> CreateFromMethod()
        {
            var idata = Expression.Parameter(typeof(IData));
            var data = Expression.Variable(typeof(Data<>).MakeGenericType(Type2));
            var value = Expression.Variable(typeof(T));

            List<Expression> list = new List<Expression>();

            if (TransformerHelper.IsEqualsTypes(Type1, Type2))
                list.Add(Expression.Label(Expression.Label(Type1), Expression.Convert(idata, data.Type).Value()));
            else
            {
                list.Add(Expression.Assign(data, Expression.Convert(idata, data.Type)));
                list.Add(TransformerHelper.BuildBody(value, data.Value(), MembersOrder1, MembersOrder2));
                list.Add(Expression.Label(Expression.Label(Type1), value));
            }

            return Expression.Lambda<Func<IData, T>>(TransformerHelper.IsEqualsTypes(Type1, Type2) ? list[0] : Expression.Block(Type1, new ParameterExpression[] { data, value }, list), idata);
        }


        public IData To(T value1)
        {
            return to(value1);
        }

        public T From(IData value2)
        {
            return from(value2);
        }
    }

    //public class Tick
    //{
    //    public string Symbol { get; set; }
    //    public DateTime Timestamp { get; set; }
    //    public double Bid { get; set; }
    //    public double Ask { get; set; }
    //    public long Volume { get; set; }
    //    public string Provider { get; set; }

    //    public Tick()
    //    {
    //    }

    //    public Tick(string symbol, DateTime time, double bid, double ask, long volume, string provider)
    //    {
    //        Symbol = symbol;
    //        Timestamp = time;
    //        Bid = bid;
    //        Ask = ask;
    //        Volume = volume;
    //        Provider = provider;
    //    }

    //    public override string ToString()
    //    {
    //        return String.Format("{0};{1:yyyy-MM-dd HH:mm:ss};{2};{3};{4};{5}", Symbol, Timestamp, Bid, Ask, Volume, Provider);
    //    }
    //}

    //public class TickNew
    //{
    //    public string SymbolNew { get; set; }
    //    public DateTime TimestampNew { get; set; }
    //    public decimal BidNew { get; set; }
    //    public decimal AskNew { get; set; }
    //    public ulong VolumeNew { get; set; }
    //    public string ProviderNew { get; set; }

    //    public TickNew()
    //    {
    //    }

    //    public TickNew(string symbolNew, DateTime timeNew, decimal bidNew, decimal askNew, ulong volumeNew, string providerNew)
    //    {
    //        SymbolNew = symbolNew;
    //        TimestampNew = timeNew;
    //        BidNew = bidNew;
    //        AskNew = askNew;
    //        VolumeNew = volumeNew;
    //        ProviderNew = providerNew;
    //    }

    //    public override string ToString()
    //    {
    //        return String.Format("{0};{1:yyyy-MM-dd HH:mm:ss};{2};{3};{4};{5}", SymbolNew, TimestampNew, BidNew, AskNew, VolumeNew, ProviderNew);
    //    }
    //}

    //public class Slotes
    //{
    //    public string Slot0 { get; set; }
    //    public DateTime Slot1 { get; set; }
    //    public double Slot2 { get; set; }
    //    public double Slot3 { get; set; }
    //    public long Slot4 { get; set; }
    //    public string Slot5 { get; set; }

    //    public Slotes()
    //    {
    //    }

    //    public Slotes(string slot0, DateTime slot1, double slot2, double slot3, long slot4, string slot5)
    //    {
    //        Slot0 = slot0;
    //        Slot1 = slot1;
    //        Slot2 = slot2;
    //        Slot3 = slot3;
    //        Slot4 = slot4;
    //        Slot5 = slot5;
    //    }
    //}

    //public class Bar
    //{
    //    public string Symbol { get; set; }
    //    public DateTime Timestamp { get; set; }
    //    public int Open { get; set; }
    //    public int High { get; set; }
    //    public int Low { get; set; }
    //    public int Close { get; set; }
    //    public Dictionary<int, string> Periodicitys { get; set; }
    //    public Dictionary<string, int> Digits { get; set; }
    //}

    //public class BarNew
    //{
    //    public string Symbol { get; set; }
    //    public DateTime Timestamp { get; set; }
    //    public sbyte OpenNew { get; set; }
    //    public double HighNew { get; set; }
    //    public long LowNew { get; set; }
    //    public float CloseNew { get; set; }
    //    public Dictionary<int, string> Periodicitys { get; set; }
    //    public Dictionary<string, long> DigitsNew { get; set; }
    //}

    //public class TickSlotesTransformer : ITransforemer<Tick, IData>
    //{
    //    public IData To(Tick value1)
    //    {
    //        Data<Slotes> dataSlotes = new Data<Slotes>();

    //        if (value1 != null)
    //        {
    //            Slotes slotes = new Slotes();

    //            slotes.Slot0 = value1.Symbol;
    //            slotes.Slot1 = value1.Timestamp;
    //            slotes.Slot2 = value1.Bid;
    //            slotes.Slot3 = value1.Ask;
    //            slotes.Slot4 = value1.Volume;
    //            slotes.Slot5 = value1.Provider;

    //            dataSlotes.Value = slotes;
    //        }
    //        else
    //            dataSlotes.Value = null;

    //        return dataSlotes;
    //    }

    //    public Tick From(IData value2)
    //    {
    //        Data<Slotes> dataSlotes = (Data<Slotes>)value2;

    //        Slotes slotes = dataSlotes.Value;
    //        Tick tick;

    //        if (slotes != null)
    //        {
    //            tick = new Tick();
    //            tick.Symbol = slotes.Slot0;
    //            tick.Timestamp = slotes.Slot1;
    //            tick.Bid = slotes.Slot2;
    //            tick.Ask = slotes.Slot3;
    //            tick.Volume = slotes.Slot4;
    //            tick.Provider = slotes.Slot5;
    //        }
    //        else
    //            tick = null;

    //        return tick;
    //    }
    //}

    //public class TickTickNewTranformer : ITransforemer<Tick, IData>
    //{
    //    public IData To(Tick value1)
    //    {
    //        Data<TickNew> dataTickNew = new Data<TickNew>();

    //        if (value1 != null)
    //        {
    //            TickNew tickNew = new TickNew();
    //            tickNew.SymbolNew = value1.Symbol;
    //            tickNew.TimestampNew = value1.Timestamp;
    //            tickNew.BidNew = (decimal)value1.Bid;
    //            tickNew.AskNew = (decimal)value1.Ask;
    //            tickNew.VolumeNew = (ulong)value1.Volume;
    //            tickNew.ProviderNew = value1.Provider;

    //            dataTickNew.Value = tickNew;
    //        }
    //        else
    //            dataTickNew.Value = null;

    //        return dataTickNew;
    //    }

    //    public Tick From(IData value2)
    //    {
    //        Data<TickNew> dataTickNew = (Data<TickNew>)value2;

    //        TickNew tickNew = dataTickNew.Value;
    //        Tick tick;
    //        if (tickNew != null)
    //        {
    //            tick = new Tick();
    //            tick.Symbol = tickNew.SymbolNew;
    //            tick.Timestamp = tickNew.TimestampNew;
    //            tick.Bid = (double)tickNew.BidNew;
    //            tick.Ask = (double)tickNew.AskNew;
    //            tick.Volume = (long)tickNew.VolumeNew;
    //            tick.Provider = tickNew.ProviderNew;
    //        }
    //        else
    //            tick = null;

    //        return tick;
    //    }
    //}

    //public class TickTickTranformer : ITransforemer<Tick, IData>
    //{
    //    public IData To(Tick value1)
    //    {
    //        return new Data<Tick>(value1);
    //    }

    //    public Tick From(IData value2)
    //    {
    //        return ((Data<Tick>)value2).Value;
    //    }
    //}

    //public class BarBarNewTransformer : ITransforemer<Bar, IData>
    //{
    //    public IData To(Bar value1)
    //    {
    //        Data<BarNew> dataBarNew = new Data<BarNew>();

    //        if (value1 != null)
    //        {
    //            BarNew barNew = new BarNew();

    //            barNew.Symbol = value1.Symbol;
    //            barNew.Timestamp = value1.Timestamp;
    //            barNew.OpenNew = (sbyte)value1.Open;
    //            barNew.HighNew = (double)value1.High;
    //            barNew.LowNew = (long)value1.Low;
    //            barNew.CloseNew = (float)value1.Close;
    //            barNew.Periodicitys = value1.Periodicitys;

    //            if (value1.Digits != null)
    //            {
    //                barNew.DigitsNew = new Dictionary<string, long>();

    //                foreach (var digit in value1.Digits)
    //                    barNew.DigitsNew.Add(digit.Key, (long)digit.Value);
    //            }
    //            else
    //                barNew.DigitsNew = null;

    //            dataBarNew.Value = barNew;
    //        }
    //        else
    //            dataBarNew.Value = null;

    //        return dataBarNew;
    //    }

    //    public Bar From(IData value2)
    //    {
    //        Data<BarNew> dataBarNew = (Data<BarNew>)value2;
    //        Bar bar;

    //        BarNew barNew = dataBarNew.Value;
    //        if (barNew != null)
    //        {
    //            bar = new Bar();

    //            bar.Symbol = barNew.Symbol;
    //            bar.Timestamp = barNew.Timestamp;
    //            bar.Open = (int)barNew.OpenNew;
    //            bar.High = (int)barNew.HighNew;
    //            bar.Low = (int)barNew.LowNew;
    //            bar.Close = (int)barNew.CloseNew;
    //            bar.Periodicitys = barNew.Periodicitys;

    //            if (barNew.DigitsNew != null)
    //            {
    //                bar.Digits = new Dictionary<string, int>();

    //                foreach (var digit in barNew.DigitsNew)
    //                    bar.Digits.Add(digit.Key, (int)digit.Value);
    //            }
    //            else
    //                bar.Digits = null;
    //        }
    //        else
    //            bar = null;

    //        return bar;
    //    }
    //}
}

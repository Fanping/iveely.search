using System;

namespace NDatabase.Meta.Compare
{
    internal static class AttributeValueComparator
    {
        /// <summary>
        ///   The following method compares any 2 objects and test if they are equal.
        ///   It will also compare equality of numbers in a very special way.
        ///   Examples:
        ///   IsEqual(Int64.MaxValue, Int64.MaxValue) //is true
        ///   IsEqual(Int64.MaxValue, Int64.MaxValue-1) //is false
        ///   IsEqual(123, 123.0) //is true
        ///   IsEqual(654f, 654d) //is true
        /// </summary>
        internal static int Compare(IComparable a, IComparable b)
        {
            if (IsNumber(a) && IsNumber(b))
            {
                if (IsFloatingPoint(a) || IsFloatingPoint(b))
                {
                    double da, db;
                    if (Double.TryParse(a.ToString(), out da) && Double.TryParse(b.ToString(), out db))
                        return da.CompareTo(db);
                }
                else
                {
                    if (a.ToString().StartsWith("-") || b.ToString().StartsWith("-"))
                    {
                        var a1 = Convert.ToInt64(a);
                        var b1 = Convert.ToInt64(b);
                        
                        return a1.CompareTo(b1);
                    }
                    else
                    {
                        var a1 = Convert.ToUInt64(a);
                        var b1 = Convert.ToUInt64(b);

                        return a1.CompareTo(b1);
                    }
                }
            }

            return a.CompareTo(b);
        }

        private static bool IsFloatingPoint(object value)
        {
            if (value is float)
                return true;
            if (value is double)
                return true;
            
            return value is decimal;
        }

        internal static bool IsNumber(object value)
        {
            if (value is sbyte)
                return true;
            if (value is byte)
                return true;
            if (value is short)
                return true;
            if (value is ushort)
                return true;
            if (value is int)
                return true;
            if (value is uint)
                return true;
            if (value is long)
                return true;
            if (value is ulong)
                return true;
            if (value is float)
                return true;
            if (value is double)
                return true;
            
            return value is decimal;
        }
    }
}

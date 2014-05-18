using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.General.Mathematics
{
    public static class MathUtils
    {
        private const int SIGN_MASK = ~Int32.MinValue;

        /// <summary>
        /// Returns the number of digits after the decimal point.
        /// </summary>
        public static int GetDigits(decimal value)
        {
            return (Decimal.GetBits(value)[3] & SIGN_MASK) >> 16;
        }

        /// <summary>
        /// Returns the number of digits after the decimal point;
        /// Returns -1, if it is not possible to convert the double value to decimal and back without precision loss.
        /// </summary>
        public static int GetDigits(double value)
        {
            decimal val = (decimal)value;
            double tmp = (double)val;
            if (tmp != value)
                return -1;

            return GetDigits(val);
        }

        /// <summary>
        /// Returns the number of digits after the decimal point;
        /// Returns -1, if it is not possible to convert the float value to decimal and back without precision loss.
        /// </summary>
        public static int GetDigits(float value)
        {
            decimal val = (decimal)value;
            float tmp = (float)val;
            if (tmp != value)
                return -1;

            return GetDigits(val);
        }
    }
}

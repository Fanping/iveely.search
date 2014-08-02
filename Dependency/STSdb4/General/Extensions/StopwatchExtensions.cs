using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.General.Extensions
{
    public static class StopwatchExtensions
    {
        public static double GetSpeed(this Stopwatch sw, long count)
        {
            return count / (sw.ElapsedMilliseconds / 1000.0);
        }
    }
}

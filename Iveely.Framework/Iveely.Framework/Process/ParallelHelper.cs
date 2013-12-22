using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iveely.Framework.Process
{
    public static class ParallelHelper
    {
        private static readonly ParallelOptions NoParallelOptions = new ParallelOptions();

        public static void InParallelWhile(this Action<ParallelLoopState> action, bool condition,
                                           ParallelOptions options = null)
        {
            if (options == null) options = NoParallelOptions;

            Parallel.ForEach(IterateForever(), options,
                             (ignored, loopState) =>
                             {
                                 if (!condition)
                                 {
                                     loopState.Stop();
                                 }
                                 else
                                 {
                                     action(loopState);
                                 }
                             });
        }

        private static IEnumerable<bool> IterateForever()
        {
            while (true)
            {
                yield return true;
            }
        }
    }
}

using System;
using Iveely.Framework.Network.Synchronous;

namespace Iveely.CloudComputing.Cacher
{
    class Program
    {
        static void Main()
        {
            Executor executor = new Executor();
            executor.Start();
        }
    }
}

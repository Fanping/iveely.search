using System.Threading;

namespace Iveely.CloudComputting.Client
{
    public sealed class Example : Application
    {
        public override void Run(object[] args)
        {
            int a = 1;
            int b = 2;
            double sum = Mathematics.Sum(3, args);
            DiagnosticsWrite("sum=" + sum + ".", args);
            DiagnosticsWrite("a-b=" + (a - b) + ".", args);
        }
    }
}

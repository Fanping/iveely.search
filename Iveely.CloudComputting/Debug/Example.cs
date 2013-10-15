using System.Threading;

namespace Iveely.CloudComputting.Client
{
    public sealed class Example : Application
    {
        public override void Run()
        {
            int a = 1;
            int b = 2;
            DiagnosticsWrite("a+b=" + (a + b) + ".");
            DiagnosticsWrite("a-b=" + (a - b) + ".");
        }
    }
}

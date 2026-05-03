using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace TcpServer
{
    internal static class Telemetry
    {
        public static ActivitySource MyActivitySource { get; }
        public static Meter MyMeter { get; }
        static Telemetry()
        {
            MyActivitySource = new ActivitySource("ActivitySource");
            MyMeter = new Meter("MyMeter");
        }
    }
}

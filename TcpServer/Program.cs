using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace TcpServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault())
                .AddSource("ActivitySource")
                .AddConsoleExporter()
                .Build();

            using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("MyMeter")
            .AddConsoleExporter()
            .Build();

            using (var store = new ParsingData.SimpleStore())
            { 
                using var server = new TcpServer(store);
                await server.StartAsync();
            }
        }
    }
}
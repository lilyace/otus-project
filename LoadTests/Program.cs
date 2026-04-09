using NBomber.Contracts;
using NBomber.CSharp;
using ParsingData;
using System.Text;

namespace LoadTests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //в задании было указано Step.Create, но у него нет такого метода.
            var scenario = Scenario.Create("my client-server scenario", async (context) =>
            {
                var step = Step.Run("set", context, async () =>
                {
                    using var client = new SimpleTcpClient("127.0.0.1", 8080);
                    try
                    {
                        var profile = new UserProfile
                        {
                            Id = 1,
                            CreatedAt = DateTime.Now,
                            Username = "Sasha"
                        };
                        await client.ConnectAsync();
                        await client.SetAsync("hello", profile);
                        return Response.Ok();
                    }
                    catch
                    {
                        return Response.Fail();
                    }
                });

                return Response.Ok();
            })
                .WithWarmUpDuration(TimeSpan.FromSeconds(10))
                .WithLoadSimulations(Simulation.Inject(100, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30)));

            NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
        }
    }
}

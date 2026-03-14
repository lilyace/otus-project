namespace TcpServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using (var store = new ParsingData.SimpleStore())
            {
                var server = new TcpServer(store);
                await server.StartAsync();
            }
        }
    }
}
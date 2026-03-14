using System.Net.Sockets;
using System.Text;

namespace TcpClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var data = new[]
            {
                "SET hello 123\n",
                "GET 123\n",
                "GET hello\n",
                "SET bye\n",
                "DELETE 123\n",
                "GET 123\n"
            };

            using var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await clientSocket.ConnectAsync("127.0.0.1", 8080);
                foreach (var item in data) {
                    var buffer = new byte[1024];
                    var requestData = Encoding.UTF8.GetBytes(item);
                    await clientSocket.SendAsync(requestData);
                    await clientSocket.ReceiveAsync(buffer);
                    Console.WriteLine(Encoding.UTF8.GetString(buffer));

                }
                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

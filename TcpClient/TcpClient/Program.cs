using ParsingData;
using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TcpClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var testData = new UserProfile
            {
                Id = 1,
                CreatedAt = DateTime.Now,
                Username = "Sasha"
            };
            var textTestValue = JsonSerializer.Serialize(testData);
            var data = new[]
            {
                $"SET hello {textTestValue}\n",
                $"GET {textTestValue}\n",
                "GET hello\n",
                "SET bye\n",
                "DELETE hello\n",
                "GET hello\nSET Hi test\nGET Hi\n"
            };

            using var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await clientSocket.ConnectAsync("127.0.0.1", 8080);
                foreach (var item in data) {
                    var requestData = Encoding.UTF8.GetBytes(item);
                    await clientSocket.SendAsync(requestData);                  

                }

                var buffer = new byte[1024];
                while (true)
                {
                    var resultByte = await clientSocket.ReceiveAsync(buffer);
                    if (resultByte == 0)
                        break;
                    Console.WriteLine(Encoding.UTF8.GetString(buffer));
                    Array.Clear(buffer, 0, buffer.Length);
                }
                Console.WriteLine("Done");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

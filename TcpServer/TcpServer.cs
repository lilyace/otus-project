using ParsingData;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpServer
{
    public class TcpServer
    {
        ArrayPool<byte> pool = ArrayPool<byte>.Shared;

        public async Task StartAsync()
        {
            IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
            var endpoint = new IPEndPoint(ipAddr, 8080);
            using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endpoint);
            socket.Listen();

            while (true)
            {
                var clientSocket = await socket.AcceptAsync();
                ProcessClientAsync(clientSocket);
            }
        }

        private async Task ProcessClientAsync(Socket clientSocket)
        {
            try
            {
                while (true)
                {
                    var buffer = pool.Rent(4096);
                    try
                    {
                        var bytesCount = await clientSocket.ReceiveAsync(buffer);
                        if (bytesCount == 0)
                            break;

                        var text = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        var separatorInd = text.IndexOf('\n');
                        var spanText = text.AsSpan();
                        while (separatorInd != -1)
                        {
                            var rec = spanText.Slice(0, separatorInd);

                            var result = CommandParser.Parse(rec);
                            Console.WriteLine($"command: {result.Command}, key: {result.Key}, value: {result.Value}");

                            spanText = spanText.Slice(separatorInd + 1);
                            separatorInd = spanText.IndexOf("\n");
                        }
                    }
                    finally
                    {
                        pool.Return(buffer, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }
    }
}

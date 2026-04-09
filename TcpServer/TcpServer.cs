using ParsingData;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace TcpServer
{
    public class TcpServer
    {
        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        private readonly SimpleStore _store;

        public TcpServer(SimpleStore store)
        {
            _store = store;
        }

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

                        var returnCarriage = (byte)'\n';
                        var separatorInd = Array.IndexOf(buffer, returnCarriage);
                        var spanText = buffer.AsSpan();
                        var startIndex = 0;
                        for(int i=0; i<bytesCount; i++)
                        {
                            if (buffer[i] != returnCarriage)
                                continue;
                            else
                            {
                                await ProcessDataAsync(buffer, startIndex, i, clientSocket);
                                startIndex = i;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
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

        private async Task ProcessDataAsync(byte[] buffer, int startIndex, int endIndex, Socket clientSocket)
        {
            var rec = buffer.AsSpan().Slice(startIndex, endIndex);
            var result = CommandParser.Parse(rec);

            switch (result.Command)
            {
                case "GET":
                    var value = _store.Get(result.Key.ToString());
                    byte[] answer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
                    await clientSocket.SendAsync(answer);
                    break;
                case "SET":
                    var profile = JsonSerializer.Deserialize<UserProfile>(result.Value);
                    _store.Set(result.Key.ToString(), profile);
                    await clientSocket.SendAsync(Encoding.UTF8.GetBytes("Ok"));
                    break;
                case "DELETE":
                    _store.Delete(result.Key.ToString());
                    await clientSocket.SendAsync(Encoding.UTF8.GetBytes("Ok"));
                    break;
                default:
                    await clientSocket.SendAsync(Encoding.UTF8.GetBytes("Error: Unknown command"));
                    break;
            }
        }
    }
}

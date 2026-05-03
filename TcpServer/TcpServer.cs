using ParsingData;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace TcpServer
{
    public class TcpServer: IDisposable
    {
        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        private readonly SimpleStore _store;
        private readonly SemaphoreSlim _maxConnectionsSemaphore;

        public TcpServer(SimpleStore store, int maxConnections = 100)
        {
            _store = store;
            _maxConnectionsSemaphore = new SemaphoreSlim(maxConnections, maxConnections);
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
                await _maxConnectionsSemaphore.WaitAsync();
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
                        if (bytesCount == 0 || bytesCount > 4096)
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
                _maxConnectionsSemaphore.Release();
            }
        }

        private async Task ProcessDataAsync(byte[] buffer, int startIndex, int endIndex, Socket clientSocket)
        {
            var rec = buffer.AsSpan().Slice(startIndex, endIndex);
            var result = CommandParser.Parse(rec);
            var counter = Telemetry.MyMeter.CreateCounter<int>("commandsCount");
            var histogram = Telemetry.MyMeter.CreateHistogram<long>("time");
            using (var activity = Telemetry.MyActivitySource.StartActivity("CommandProcessing", ActivityKind.Server))
            {
                activity?.SetTag("command.name", result.Command.ToString());
                var sw = new Stopwatch();
                sw.Start();
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
                sw.Stop();
                histogram.Record(sw.ElapsedMilliseconds);
                counter.Add(1);
                activity?.AddEvent(new ActivityEvent("Command handled"));
            }
        }

        public void Dispose()
        {
            _maxConnectionsSemaphore.Dispose();
        }
    }
}

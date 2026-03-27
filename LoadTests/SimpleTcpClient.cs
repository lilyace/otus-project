using System.Buffers;
using System.Net.Sockets;
using System.Text;

namespace LoadTests
{
    internal class SimpleTcpClient: IDisposable
    {
        ArrayPool<byte> pool = ArrayPool<byte>.Shared;
        private readonly string _host;
        private readonly int _port;
        private Socket _socket;

        public SimpleTcpClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async Task ConnectAsync()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);           
            await _socket.ConnectAsync(_host, _port);  
        }

        public async Task SetAsync(string key, byte[] value)
        {
            var data = Encoding.UTF8.GetBytes($"SET {key} ").Concat(value).ToArray();
            await _socket.SendAsync(data);
        }

        public async Task GetAsync(string key)
        {
            var buffer = pool.Rent(1096);
            try
            {
                await _socket.ReceiveAsync(buffer);
            }
            finally
            {
                pool.Return(buffer, true);
            }
        }

        public void Dispose() => _socket.Dispose();
    }
}

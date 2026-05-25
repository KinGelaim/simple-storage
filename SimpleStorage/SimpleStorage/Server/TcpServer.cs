using System.Net;
using System.Net.Sockets;

namespace SimpleStorage.Server;

internal sealed class TcpServer(string ip, int port) : IDisposable
{
    private Socket? _socket;
    private readonly IPAddress _ip = IPAddress.Parse(ip);
    private readonly int _port = port;

    public async Task StartAsync()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        _socket.Bind(new IPEndPoint(_ip, _port));
        _socket.Listen(backlog: 10);

        Console.WriteLine($"Сервер слушает по адресу {_ip}:{_port}");

        await Task.Delay(Timeout.Infinite);
    }

    public void Dispose() => _socket?.Dispose();
}
using SimpleStorage.Parser;
using System.Buffers;
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

        while (true)
        {
            try
            {
                var client = await _socket.AcceptAsync();

                Console.WriteLine("Клиент подключился!");

                _ = ProcessClientAsync(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения клиента: {ex.Message}");
            }
        }
    }

    private static async Task ProcessClientAsync(Socket client)
    {
        var bufferSize = 1024;
        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        try
        {
            while (true)
            {
                var bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None);

                var dataMemory = new ReadOnlySpan<byte>(buffer, 0, bytesRead);
                var commandParts = CommandParser.Parse(dataMemory);

                var command = commandParts.Command.ToString();
                var key = commandParts.Key.ToString();
                var value = commandParts.Value.ToString();
                Console.WriteLine($"Команда: {command}, Ключ: {key}, Значение: {value}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке клиента: {ex.Message}");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void Dispose() => _socket?.Dispose();
}
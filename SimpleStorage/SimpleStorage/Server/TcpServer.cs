using SimpleStorage.Parser;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleStorage.Server;

internal sealed class TcpServer(string ip, int port) : IDisposable
{
    private Socket? _socket;
    private readonly IPAddress _ip = IPAddress.Parse(ip);
    private readonly int _port = port;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        _socket.Bind(new IPEndPoint(_ip, _port));
        _socket.Listen(backlog: 10);

        Console.WriteLine($"Сервер слушает по адресу {_ip}:{_port}");

        var clientCounter = 1;
        while (true)
        {
            try
            {
                var client = await _socket.AcceptAsync(cancellationToken);

                Console.WriteLine($"Клиент {clientCounter} подключился");

                _ = ProcessClientAsync(client, clientCounter, cancellationToken);

                clientCounter++;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения клиента: {ex.Message}");
            }
        }
    }

    private static async Task ProcessClientAsync(
        Socket client,
        int clientCounter,
        CancellationToken cancellationToken)
    {
        var bufferSize = 1024;
        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        try
        {
            while (true)
            {
                var bytesRead = await client.ReceiveAsync(
                    buffer,
                    SocketFlags.None,
                    cancellationToken);
                if (bytesRead == 0)
                {
                    break;
                }

                var commandParts = CommandParser.Parse(buffer);

                var command = Encoding.UTF8.GetString(commandParts.Command);
                var key = Encoding.UTF8.GetString(commandParts.Key);
                var value = Encoding.UTF8.GetString(commandParts.Value);
                Console.WriteLine($"Команда от клиента {clientCounter}: {command}, Ключ: {key}, Значение: {value}");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке клиента: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"Отключение клиента {clientCounter}");
            ArrayPool<byte>.Shared.Return(buffer);
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }

    public void Dispose() => _socket?.Dispose();
}
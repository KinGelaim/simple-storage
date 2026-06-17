using SimpleStorage.Parser;
using SimpleStorage.Storage;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleStorage.Server;

internal sealed class TcpServer(string ip, int port, SimpleStore store) : IDisposable
{
    private Socket? _socket;
    private readonly IPAddress _ip = IPAddress.Parse(ip);
    private readonly int _port = port;
    private readonly SimpleStore _store = store;

    private readonly byte[] _successResponse = Encoding.UTF8.GetBytes("OK\r\n");
    private readonly byte[] _notFoundResponse = Encoding.UTF8.GetBytes("(nil)\r\n");
    private readonly byte[] _errorResponse = Encoding.UTF8.GetBytes("-ERR Unknown command\r\n");

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

    private async Task ProcessClientAsync(
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

#if DEBUG
                var textCommand = Encoding.UTF8.GetString(commandParts.Command);
                var textKey = Encoding.UTF8.GetString(commandParts.Key);
                var textValue = Encoding.UTF8.GetString(commandParts.Value);
                Console.WriteLine($"Команда от клиента {clientCounter}: {textCommand}, Ключ: {textKey}, Значение: {textValue}");
#endif

                byte[] response;
                var command = Encoding.UTF8.GetString(commandParts.Command);
                var key = Encoding.UTF8.GetString(commandParts.Key);
                switch (command)
                {
                    case "GET":
                        var result = _store.Get(key);
                        response = result is not null
                            ? result
                            : _notFoundResponse;
                        break;
                    case "SET":
                        var value = commandParts.Value.ToArray();
                        _store.Set(key, value);
                        response = _successResponse;
                        break;
                    case "DELETE":
                        _store.Delete(key);
                        response = _successResponse;
                        break;
                    default:
                        response = _errorResponse;
                        break;
                }

                await client.SendAsync(response, cancellationToken);
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
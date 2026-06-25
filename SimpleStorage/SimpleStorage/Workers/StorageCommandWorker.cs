using SimpleStorage.Models;
using SimpleStorage.Services;
using SimpleStorage.Storage;
using System.Text;

namespace SimpleStorage.Workers;

/// <summary>
/// Обработчик для команд
/// </summary>
/// <param name="commandChannelService">Сервис хранящий канал для передачи команд</param>
/// <param name="storage">Хранилище данных</param>
internal sealed class StorageCommandWorker(
    CommandChannelService commandChannelService,
    SimpleStore storage)
{
    private readonly CommandChannelService _commandChannelService = commandChannelService;
    private readonly SimpleStore _storage = storage;

    private readonly byte[] _successResponse = Encoding.UTF8.GetBytes("OK\r\n");
    private readonly byte[] _notFoundResponse = Encoding.UTF8.GetBytes("(nil)\r\n");
    private readonly byte[] _errorResponse = Encoding.UTF8.GetBytes("-ERR Unknown command\r\n");
    private readonly byte[] _end = [(byte)'\r', (byte)'\n'];

    /// <summary>
    /// Обработчик входящих команд из сервиса канала
    /// </summary>
    /// <param name="cancellationToken">Токен завершения асинхронной работы</param>
    public async Task ProcessCommandsAsync(CancellationToken cancellationToken)
    {
        await foreach (var commandContext in _commandChannelService.Reader.ReadAllAsync(cancellationToken))
        {
            var command = commandContext.ParsedCommand;
            if (command is null)
            {
                commandContext.ResponseTcs.SetResult(_errorResponse);
                continue;
            }

            byte[] response;
            switch (command.Type)
            {
                case CommandType.Get:
                    var value = _storage.Get(command.Key);
                    if (value is not null)
                    {
                        response = new byte[value.Length + _end.Length];
                        Array.Copy(value, response, value.Length);
                        Array.Copy(_end, 0, response, value.Length, _end.Length);
                    }
                    else
                    {
                        response = _notFoundResponse;
                    }
                    break;
                case CommandType.Set:
                    _storage.Set(command.Key, command.Value);
                    response = _successResponse;
                    break;
                case CommandType.Delete:
                    _storage.Delete(command.Key);
                    response = _successResponse;
                    break;
                default:
                    response = _errorResponse;
                    break;
            }
            commandContext.ResponseTcs.SetResult(response);
        }
    }
}
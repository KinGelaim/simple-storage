using SimpleStorage.Models;
using System.Threading.Channels;

namespace SimpleStorage.Services;

/// <summary>
/// Сервис для хранения канала передачи команд
/// </summary>
internal class CommandChannelService
{
    private readonly Channel<CommandContext> _channel;

    public ChannelReader<CommandContext> Reader => _channel.Reader;
    public ChannelWriter<CommandContext> Writer => _channel.Writer;

    public CommandChannelService() => _channel = Channel.CreateUnbounded<CommandContext>();
}
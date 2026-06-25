namespace SimpleStorage.Models;

/// <summary>
/// Контекст команды
/// </summary>
/// <param name="command">Команда</param>
internal sealed class CommandContext(Command? command)
{
    /// <summary>
    /// Команда
    /// </summary>
    public Command? ParsedCommand { get; init; } = command;

    /// <summary>
    /// Обратная связь
    /// </summary>
    public TaskCompletionSource<byte[]> ResponseTcs { get; init; } = new();
}
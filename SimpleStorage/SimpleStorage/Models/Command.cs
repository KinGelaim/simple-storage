namespace SimpleStorage.Models;

/// <summary>
/// Информация о команде
/// </summary>
internal sealed class Command
{
    /// <summary>
    /// Тип команды
    /// </summary>
    public required CommandType Type { get; init; }

    /// <summary>
    /// Ключ
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Значение
    /// </summary>
    public required byte[] Value { get; init; }
}
namespace SimpleStorage.Models;

/// <summary>
/// Составные части команды
/// </summary>
internal ref struct CommandParts
{
    /// <summary>
    /// Команда
    /// </summary>
    public ReadOnlySpan<byte> Command { get; set; }

    /// <summary>
    /// Ключ
    /// </summary>
    public ReadOnlySpan<byte> Key { get; set; }

    /// <summary>
    /// Значение
    /// </summary>
    public ReadOnlySpan<byte> Value { get; set; }
}
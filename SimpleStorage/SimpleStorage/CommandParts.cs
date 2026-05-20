namespace SimpleStorage;

/// <summary>
/// Составные части команды
/// </summary>
internal ref struct CommandParts
{
    /// <summary>
    /// Команда
    /// </summary>
    public ReadOnlySpan<byte> Command;

    /// <summary>
    /// Ключ
    /// </summary>
    public ReadOnlySpan<byte> Key;

    /// <summary>
    /// Значение
    /// </summary>
    public ReadOnlySpan<byte> Value;
}
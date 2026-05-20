namespace SimpleStorage;

/// <summary>
/// Парсер команд
/// </summary>
internal static class CommandParser
{
    /// <summary>
    /// Парсер команды
    /// </summary>
    /// <param name="bytes">Входящая последовательность байт вида "COMMAND KEY VALUE"</param>
    public static CommandParts Parse(ReadOnlySpan<byte> bytes)
    {
        return new CommandParts();
    }
}
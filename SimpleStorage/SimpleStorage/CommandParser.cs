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
        var result = new CommandParts();
        var byteSpace = (byte)' ';
        var trimBytes = bytes.Trim(byteSpace);

        var firstSpace = trimBytes.IndexOf(byteSpace);
        if (firstSpace == -1)
        {
            return result;
        }

        result.Command = trimBytes[..firstSpace];

        var remaining = trimBytes[(firstSpace + 1)..].Trim(byteSpace);

        var secondSpace = remaining.IndexOf(byteSpace);
        if (secondSpace == -1)
        {
            result.Key = remaining;
            result.Value = [];
            return result;
        }

        result.Key = remaining[..secondSpace];
        result.Value = remaining[(secondSpace + 1)..].Trim(byteSpace);
        return result;
    }
}
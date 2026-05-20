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

        var firstSpace = bytes.IndexOf(byteSpace);
        if (firstSpace == -1)
        {
            return result;
        }

        result.Command = bytes.Slice(0, firstSpace);

        var remaining = bytes.Slice(firstSpace + 1).Trim(byteSpace);

        var secondSpace = remaining.IndexOf(byteSpace);
        if (secondSpace == -1)
        {
            result.Key = remaining;
            result.Value = ReadOnlySpan<byte>.Empty;
        }
        else
        {
            result.Key = remaining.Slice(0, secondSpace);
            result.Value = remaining.Slice(secondSpace + 1).Trim(byteSpace);
        }

        return result;
    }
}
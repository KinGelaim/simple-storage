using SimpleStorage.Models;

namespace SimpleStorage.Parser;

/// <summary>
/// Парсер команд
/// </summary>
internal static class CommandParser
{
    private static readonly byte _byteSpace = (byte)' ';
    private static readonly byte[] _end = [(byte)'\r', (byte)'\n'];

    public static int? GetPosition(ReadOnlySpan<byte> buffer)
    {
        var position = buffer.IndexOf(_end);
        return position == -1
            ? null
            : position + _end.Length;
    }

    /// <summary>
    /// Парсер команды
    /// </summary>
    /// <param name="bytes">Входящая последовательность байт вида "COMMAND KEY VALUE"</param>
    public static CommandParts Parse(ReadOnlySpan<byte> bytes)
    {
        var result = new CommandParts();
        var trimBytes = bytes.Trim(_end).Trim(_byteSpace);

        var firstSpace = trimBytes.IndexOf(_byteSpace);
        if (firstSpace == -1)
        {
            return result;
        }

        result.Command = trimBytes[..firstSpace];

        var remaining = trimBytes[(firstSpace + 1)..].Trim(_byteSpace);

        var secondSpace = remaining.IndexOf(_byteSpace);
        if (secondSpace == -1)
        {
            result.Key = remaining;
            result.Value = [];
            return result;
        }

        result.Key = remaining[..secondSpace];
        result.Value = remaining[(secondSpace + 1)..].Trim(_byteSpace);
        return result;
    }
}
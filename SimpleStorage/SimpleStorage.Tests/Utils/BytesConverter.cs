using System.Text;

namespace SimpleStorage.Tests.Utils;

internal static class BytesConverter
{
    public static string ToString(this ReadOnlySpan<byte> bytes)
        => Encoding.UTF8.GetString(bytes);
}
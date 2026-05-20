using System.Text;

namespace SimpleStorage.Tests.Utils;

internal static class StringExtensions
{
    public static byte[] ToBytes(this string str) =>
        Encoding.UTF8.GetBytes(str);
}
using System.Security.Cryptography;
using System.Text;

namespace Common;

public static class HashHelper
{
    public static string GetHash(string input)
    {
        var data = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        var sb = new StringBuilder();
        foreach (var t in data)
        {
            sb.Append(t.ToString("x2"));
        }

        return sb.ToString();
    }

    public static bool Verify(string input, string hash)
    {
        var hashInput = GetHash(input);
        var comparer = StringComparer.OrdinalIgnoreCase;
        return comparer.Compare(hashInput, hash) == 0;
    }
}
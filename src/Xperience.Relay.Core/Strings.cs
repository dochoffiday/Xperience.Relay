using System.Text.RegularExpressions;

namespace Xperience.Relay.Core;

public static class Strings
{
    /// <summary>
    /// Sanitises a string so it can be used as a code‑name.
    /// Allowed characters: a‑z, A‑Z, 0‑9, '_', '-', '.'
    /// Spaces are turned into '-', and the result cannot start or end with '.'.
    /// </summary>
    public static string ToCodeName(string input)
    {
        if (input == null) { return ""; }

        var trimmed = input.Trim();

        // 1. Replace spaces with '-'
        var replaced = trimmed.Replace(' ', '-');

        // 2. Remove illegal characters – keep only alphanumerics, _, -, .
        //    Using a regular expression that matches anything NOT in the allowed set.
        var cleaned = Regex.Replace(replaced, @"[^A-Za-z0-9_\-\.]", string.Empty);

        // 3. Trim leading / trailing '.' characters (if any)
        var final = cleaned.Trim('.');

        return final;
    }

    /// <summary>
    /// Shortens a string to the specified length, returning the original string if it's shorter than or equal to that length.
    /// </summary>
    public static string TrimLength(this string input, int length)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return input.Length > length
                ? input[..length]
                : input;
    }
}

using System.Text.Json;

namespace Xperience.Relay.Kentico.Handlers;

internal static class QueryItemsHelpers
{
    internal static Dictionary<string, JsonElement> ExtractRow(
        List<string> columns,
        TryGetValueDelegate tryGetValue)
    {
        var dict = new Dictionary<string, JsonElement>(columns.Count);
        foreach (var col in columns)
        {
            if (tryGetValue(col, out var val))
            {
                // Kentico serializes tag/linked-item fields as JSON strings internally.
                // Embedding them directly avoids double-encoding (string-of-a-string).
                if (val is string s && s.Length > 0 && (s[0] == '[' || s[0] == '{'))
                {
                    try
                    {
                        dict[col] = JsonDocument.Parse(s).RootElement.Clone();
                        continue;
                    }
                    catch (JsonException) { }
                }

                dict[col] = JsonSerializer.SerializeToElement(val);
            }
        }
        return dict;
    }

    internal delegate bool TryGetValueDelegate(string key, out object? value);

    internal static object? GetScalarValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number when element.TryGetInt32(out var i) => i,
        JsonValueKind.Number when element.TryGetInt64(out var l) => l,
        JsonValueKind.Number => element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };

    internal static object? DeserializeJsonElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number when element.TryGetInt32(out var i) => i,
        JsonValueKind.Number when element.TryGetInt64(out var l) => l,
        JsonValueKind.Number => element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => element.ToString()
    };
}

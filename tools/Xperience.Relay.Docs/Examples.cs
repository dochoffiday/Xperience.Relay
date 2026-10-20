using System.Text.Encodings.Web;
using System.Text.Json;

namespace Xperience.Relay.Docs;

/// <summary>One captured request and response: <c>docs/examples/&lt;verb&gt;/&lt;name&gt;.json</c>.</summary>
sealed record Example(string Name, string Title, string? Note, bool IsFailure, int Status, int Order, JsonElement Parameters, JsonElement Response);

static class ExampleStore
{
    private static readonly JsonSerializerOptions Pretty = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static List<Example> Load(string examplesRoot, string verb, List<string> problems)
    {
        var directory = Path.Combine(examplesRoot, verb);
        if (!Directory.Exists(directory))
        {
            return [];
        }

        var examples = new List<Example>();

        foreach (var file in Directory.GetFiles(directory, "*.json"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var source = $"docs/examples/{verb}/{name}.json";

            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(file));
                var root = document.RootElement;
                var request = root.GetProperty("request");

                if (request.GetProperty("verb").GetString() != verb)
                {
                    problems.Add($"{source}: the request's verb isn't '{verb}'.");
                    continue;
                }

                examples.Add(new Example(
                    name,
                    root.GetProperty("title").GetString()!,
                    root.TryGetProperty("note", out var note) ? note.GetString() : null,
                    root.TryGetProperty("kind", out var kind) && kind.GetString() == "error",
                    root.GetProperty("status").GetInt32(),
                    root.TryGetProperty("order", out var order) ? order.GetInt32() : int.MaxValue,
                    request.GetProperty("parameters").Clone(),
                    root.GetProperty("response").Clone()));
            }
            catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
            {
                problems.Add($"{source}: {ex.Message}");
            }
        }

        return examples.OrderBy(e => e.Order).ThenBy(e => e.Name, StringComparer.Ordinal).ToList();
    }

    /// <summary>The element as indented JSON, in the order it was written, without escaping quotes and angle brackets.</summary>
    public static string Indented(JsonElement element) =>
        JsonSerializer.Serialize(element, Pretty).Replace("\r\n", "\n");

    /// <summary>The envelope a client posts: the verb and its parameters.</summary>
    public static string Envelope(string verb, JsonElement parameters) =>
        Indented(JsonSerializer.SerializeToElement(new Dictionary<string, JsonElement>
        {
            ["verb"] = JsonSerializer.SerializeToElement(verb),
            ["parameters"] = parameters,
        }));
}

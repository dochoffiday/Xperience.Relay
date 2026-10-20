using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Xperience.Relay.Docs;

/// <summary>
/// Writes the C# that sends a command, built from the very parameters the HTTP example sends: the JSON is read into the
/// real command type, so an example that no longer fits the command is caught when the docs are generated.
/// </summary>
static class CSharpSnippet
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string For(CommandModel command, JsonElement parameters, List<string> problems, string source)
    {
        object? instance;
        try
        {
            instance = parameters.Deserialize(command.Type, Options);
        }
        catch (JsonException ex)
        {
            problems.Add($"{source}: the parameters don't fit {command.Type.Name}: {ex.Message}");
            return string.Empty;
        }

        var assigned = new List<(PropertyModel Property, object? Value)>();

        if (parameters.ValueKind == JsonValueKind.Object)
        {
            foreach (var member in parameters.EnumerateObject())
            {
                var property = command.Properties.FirstOrDefault(p => string.Equals(p.Name, member.Name, StringComparison.OrdinalIgnoreCase));
                if (property is null)
                {
                    problems.Add($"{source}: {command.Type.Name} has no parameter '{member.Name}'.");
                    continue;
                }

                assigned.Add((property, property.Property.GetValue(instance)));
            }
        }

        var text = new StringBuilder();
        if (assigned.Count == 0)
        {
            text.Append($"var result = await client.ExecuteAsync(new {command.Type.Name}());");
        }
        else
        {
            text.AppendLine($"var result = await client.ExecuteAsync(new {command.Type.Name}");
            text.AppendLine("{");
            foreach (var (property, value) in assigned)
            {
                text.Append("    ").Append(property.Name).Append(" = ").Append(Expression(value, 1)).AppendLine(",");
            }

            text.Append("});");
        }

        if (command.ResultType is { } resultType)
        {
            text.AppendLine().Append($"var data = result.GetData<{resultType.Name}>();");
        }

        return text.ToString();
    }

    private static string Expression(object? value, int level)
    {
        var pad = new string(' ', level * 4);

        switch (value)
        {
            case null:
                return "null";
            case string s:
                return Literal(s);
            case bool b:
                return b ? "true" : "false";
            case int or long or short or byte:
                return Convert.ToString(value, CultureInfo.InvariantCulture)!;
            case decimal d:
                return d.ToString(CultureInfo.InvariantCulture) + "m";
            case double or float:
                return Convert.ToString(value, CultureInfo.InvariantCulture)!;
            case Guid g:
                return $"Guid.Parse(\"{g}\")";
            case JsonElement element:
                return Json(element);
            case IDictionary dictionary:
            {
                if (dictionary.Count == 0)
                {
                    return "new()";
                }

                var text = new StringBuilder("new()").AppendLine().Append(pad).AppendLine("{");
                foreach (DictionaryEntry entry in dictionary)
                {
                    text.Append(pad).Append("    [").Append(Literal((string)entry.Key)).Append("] = ").Append(Expression(entry.Value, level + 1)).AppendLine(",");
                }

                return text.Append(pad).Append('}').ToString();
            }

            case IList list:
            {
                var items = list.Cast<object?>().ToList();
                if (items.Count == 0)
                {
                    return "[]";
                }

                if (items.All(i => i is string or int or long or bool or Guid))
                {
                    return "[" + string.Join(", ", items.Select(i => Expression(i, level + 1))) + "]";
                }

                var text = new StringBuilder("[").AppendLine();
                foreach (var item in items)
                {
                    text.Append(pad).Append("    ").Append(Expression(item, level + 1)).AppendLine(",");
                }

                return text.Append(pad).Append(']').ToString();
            }

            default:
            {
                // A nested object such as a RelayAsset: set the properties that have a value.
                var properties = value.GetType().GetProperties()
                    .Where(p => p.CanRead && p.CanWrite && p.GetValue(value) is { } v && !IsEmpty(v))
                    .ToList();

                var text = new StringBuilder($"new {value.GetType().Name}").AppendLine().Append(pad).AppendLine("{");
                foreach (var property in properties)
                {
                    text.Append(pad).Append("    ").Append(property.Name).Append(" = ").Append(Expression(property.GetValue(value), level + 1)).AppendLine(",");
                }

                return text.Append(pad).Append('}').ToString();
            }
        }
    }

    private static bool IsEmpty(object value) => value is string { Length: 0 };

    private static string Json(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => $"JsonSerializer.SerializeToElement({Literal(element.GetString()!)})",
        JsonValueKind.Number => $"JsonSerializer.SerializeToElement({element.GetRawText()})",
        JsonValueKind.True => "JsonSerializer.SerializeToElement(true)",
        JsonValueKind.False => "JsonSerializer.SerializeToElement(false)",
        JsonValueKind.Null => "JsonSerializer.SerializeToElement<string?>(null)",
        _ => "JsonDocument.Parse(" + Literal(element.GetRawText()) + ").RootElement",
    };

    private static string Literal(string value)
    {
        var text = new StringBuilder("\"");
        foreach (var c in value)
        {
            text.Append(c switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                _ when char.IsControl(c) => $"\\u{(int)c:x4}",
                _ => c.ToString(),
            });
        }

        return text.Append('"').ToString();
    }
}

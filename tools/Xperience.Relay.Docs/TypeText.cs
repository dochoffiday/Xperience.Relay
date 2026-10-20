using System.Text.Json;

namespace Xperience.Relay.Docs;

/// <summary>Describes a .NET type the way it looks in the JSON that travels over the wire.</summary>
static class TypeText
{
    /// <summary>
    /// Short notation used in the tables: <c>string</c>, <c>integer</c>, <c>guid</c>, <c>string[]</c>,
    /// <c>{ name: any }</c>, or a link to a documented type.
    /// </summary>
    public static string Of(Type type, Catalog catalog, Links links)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(string))
        {
            return "string";
        }

        if (type == typeof(int) || type == typeof(long) || type == typeof(short))
        {
            return "integer";
        }

        if (type == typeof(bool))
        {
            return "boolean";
        }

        if (type == typeof(double) || type == typeof(decimal) || type == typeof(float))
        {
            return "number";
        }

        if (type == typeof(Guid))
        {
            return "guid";
        }

        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            return "datetime";
        }

        if (type == typeof(JsonElement) || type == typeof(object))
        {
            return "any";
        }

        if (type.IsEnum)
        {
            return "string";
        }

        if (type.IsGenericType)
        {
            var definition = type.GetGenericTypeDefinition();
            var arguments = type.GetGenericArguments();

            if (definition == typeof(List<>) || definition == typeof(IEnumerable<>) || definition == typeof(IReadOnlyList<>))
            {
                return Of(arguments[0], catalog, links) + "[]";
            }

            if (definition == typeof(Dictionary<,>))
            {
                return "{ name: " + Of(arguments[1], catalog, links) + " }";
            }
        }

        var documented = catalog.TypeFor(type);
        return documented is null ? $"`{type.Name}`" : $"[{type.Name}]({links.ToType(documented)})";
    }
}

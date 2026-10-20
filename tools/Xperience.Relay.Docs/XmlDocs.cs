using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Xperience.Relay.Docs;

/// <summary>Reads the compiler's XML documentation file for the Contracts package.</summary>
sealed class XmlDocs
{
    private readonly Dictionary<string, XElement> _members;

    private XmlDocs(Dictionary<string, XElement> members) => _members = members;

    public static XmlDocs Load(string path)
    {
        var doc = XDocument.Load(path);
        var members = doc.Descendants("member")
            .Where(m => m.Attribute("name") is not null)
            .ToDictionary(m => (string)m.Attribute("name")!, m => m);
        return new XmlDocs(members);
    }

    /// <summary>The raw <c>&lt;summary&gt;</c> element of a type or property, if it has one.</summary>
    public XElement? SummaryOf(MemberInfo member) =>
        _members.TryGetValue(KeyOf(member), out var element) ? element.Element("summary") : null;

    private static string KeyOf(MemberInfo member) => member switch
    {
        Type type => "T:" + type.FullName!.Replace('+', '.'),
        PropertyInfo property => "P:" + property.DeclaringType!.FullName!.Replace('+', '.') + "." + property.Name,
        _ => throw new NotSupportedException($"No XML doc key for {member}."),
    };
}

/// <summary>How the inline markup of a summary is written out as Markdown (links depend on the page being written).</summary>
interface IMarkdownLinks
{
    /// <summary>A <c>cref</c> such as <c>T:Xperience.Relay.Contracts.Commands.CreateWebPageCommand</c>.</summary>
    string Cref(string cref);

    /// <summary>The text of a code span (<c>&lt;c&gt;</c>).</summary>
    string Code(string text);

    /// <summary>Plain text that has already been escaped for Markdown.</summary>
    string Plain(string text);
}

/// <summary>Turns the inline markup used in the summaries (<c>c</c>, <c>see</c>, <c>paramref</c>) into Markdown.</summary>
static class XmlText
{
    public static string ToMarkdown(XElement? summary, IMarkdownLinks links)
    {
        if (summary is null)
        {
            return string.Empty;
        }

        var text = new StringBuilder();
        foreach (var node in summary.Nodes())
        {
            Append(text, node, links);
        }

        return Regex.Replace(text.ToString(), @"\s+", " ").Trim();
    }

    /// <summary>The first sentence of a Markdown text (an abbreviation such as "e.g." doesn't end it).</summary>
    public static string FirstSentence(string markdown)
    {
        for (var i = 0; i < markdown.Length - 1; i++)
        {
            if (markdown[i] != '.' || markdown[i + 1] != ' ')
            {
                continue;
            }

            var before = markdown[..i];
            if (before.EndsWith("e.g", StringComparison.OrdinalIgnoreCase) || before.EndsWith("i.e", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Don't stop inside a code span (`...`): count the backticks before the full stop.
            if (before.Count(c => c == '`') % 2 == 1)
            {
                continue;
            }

            return markdown[..(i + 1)];
        }

        return markdown;
    }

    private static void Append(StringBuilder text, XNode node, IMarkdownLinks links)
    {
        switch (node)
        {
            case XText plain:
                text.Append(links.Plain(EscapePlain(plain.Value)));
                break;
            case XElement { Name.LocalName: "c" } code:
                text.Append(links.Code(Regex.Replace(code.Value, @"\s+", " ").Trim()));
                break;
            case XElement { Name.LocalName: "see" } see:
                var cref = (string?)see.Attribute("cref");
                var langword = (string?)see.Attribute("langword");
                if (langword is not null)
                {
                    text.Append('`').Append(langword).Append('`');
                }
                else if (cref is not null)
                {
                    text.Append(links.Cref(cref));
                }

                break;
            case XElement { Name.LocalName: "paramref" or "typeparamref" } reference:
                text.Append('`').Append((string?)reference.Attribute("name")).Append('`');
                break;
            case XElement other:
                text.Append(links.Plain(EscapePlain(other.Value)));
                break;
        }
    }

    // Plain text goes into Markdown tables and paragraphs: keep it from being read as HTML or as a column break.
    private static string EscapePlain(string value) =>
        value.Replace("<", "&lt;").Replace(">", "&gt;").Replace("|", "\\|");
}

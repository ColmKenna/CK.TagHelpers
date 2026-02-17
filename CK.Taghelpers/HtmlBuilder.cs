using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace CK.Taghelpers;

/// <summary>
/// A fluent builder for constructing HTML strings safely with automatic encoding.
/// </summary>
public sealed class HtmlBuilder
{
    private readonly StringBuilder _sb;

    public HtmlBuilder(int capacity = 256) => _sb = new StringBuilder(capacity);
    public HtmlBuilder(StringBuilder sb) => _sb = sb ?? throw new ArgumentNullException(nameof(sb));

    /// <summary>Opens a tag: &lt;tag</summary>
    public HtmlBuilder OpenTag(string tag)
    {
        _sb.Append('<').Append(tag);
        return this;
    }

    /// <summary>Opens a tag with optional class and id shorthand.</summary>
    public HtmlBuilder OpenTag(string tag, string? cssClass = null, string? id = null)
    {
        _sb.Append('<').Append(tag);
        if (cssClass is not null) Attr("class", cssClass);
        if (id is not null) Attr("id", id);
        return this;
    }

    /// <summary>Appends a complete opening tag: &lt;tag&gt; with optional class and id shorthand.</summary>
    public HtmlBuilder Tag(string tag, string? cssClass = null, string? id = null)
    {
        return OpenTag(tag, cssClass, id).CloseStart();
    }

    /// <summary>Appends a complete element with encoded text content.</summary>
    public HtmlBuilder Element(string tag, string text, string? cssClass = null, string? id = null)
    {
        return Tag(tag, cssClass, id).Text(text).CloseTag(tag);
    }

    /// <summary>Appends an encoded attribute: name="encoded-value"</summary>
    public HtmlBuilder Attr(string name, string value)
    {
        _sb.Append(' ').Append(name).Append("=\"")
           .Append(HtmlEncoder.Default.Encode(value))
           .Append('"');
        return this;
    }

    /// <summary>Appends multiple encoded attributes in one call. Each entry must be [name, value].</summary>
    public HtmlBuilder Attr(params string[][] attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        foreach (var attribute in attributes)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            if (attribute.Length != 2)
            {
                throw new ArgumentException(
                    "Each attribute must contain exactly two values: [name, value].",
                    nameof(attributes));
            }

            Attr(attribute[0], attribute[1]);
        }

        return this;
    }

    /// <summary>Conditionally appends an attribute.</summary>
    public HtmlBuilder AttrIf(bool condition, string name, string value)
    {
        return condition ? Attr(name, value) : this;
    }

    /// <summary>Appends a boolean attribute (e.g. disabled, hidden).</summary>
    public HtmlBuilder BoolAttr(string name)
    {
        _sb.Append(' ').Append(name);
        return this;
    }

    /// <summary>Conditionally appends a boolean attribute.</summary>
    public HtmlBuilder BoolAttrIf(bool condition, string name)
    {
        return condition ? BoolAttr(name) : this;
    }

    /// <summary>Closes the opening tag: &gt;</summary>
    public HtmlBuilder CloseStart()
    {
        _sb.Append('>');
        return this;
    }

    /// <summary>Self-closes the tag: /&gt;</summary>
    public HtmlBuilder SelfClose()
    {
        _sb.Append(" />");
        return this;
    }

    /// <summary>Appends a closing tag: &lt;/tag&gt;</summary>
    public HtmlBuilder CloseTag(string tag)
    {
        _sb.Append("</").Append(tag).Append('>');
        return this;
    }

    /// <summary>Appends HTML-encoded text content.</summary>
    public HtmlBuilder Text(string text)
    {
        _sb.Append(HtmlEncoder.Default.Encode(text));
        return this;
    }

    /// <summary>Appends pre-encoded or trusted HTML. Use with caution.</summary>
    public HtmlBuilder Raw(string html)
    {
        _sb.Append(html);
        return this;
    }

    /// <summary>Appends content from an IHtmlContent (e.g. partial view result).</summary>
    public HtmlBuilder AppendHtmlContent(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        _sb.Append(writer.ToString());
        return this;
    }

    /// <summary>Shorthand for a complete void element: &lt;input type="hidden" .../&gt;</summary>
    public HtmlBuilder HiddenInput(string name, string value)
    {
        return OpenTag("input")
            .Attr("type", "hidden")
            .Attr("name", name)
            .Attr("value", value)
            .SelfClose();
    }

    /// <summary>Shorthand for a complete button element.</summary>
    public HtmlBuilder Button(string cssClass, string text, string ariaLabel,
        IEnumerable<KeyValuePair<string, string>>? dataAttrs = null)
    {
        OpenTag("button").Attr("type", "button").Attr("class", cssClass).Attr("aria-label", ariaLabel);
        if (dataAttrs is not null)
        {
            foreach (var (key, val) in dataAttrs)
                Attr(key, val);
        }
        return CloseStart().Text(text).CloseTag("button");
    }

    /// <summary>Returns the accumulated HTML string.</summary>
    public override string ToString() => _sb.ToString();

    /// <summary>Returns the length of the accumulated content.</summary>
    public int Length => _sb.Length;

    /// <summary>Provides direct access to the underlying StringBuilder for advanced scenarios.</summary>
    public StringBuilder InnerBuilder => _sb;
}

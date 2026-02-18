using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace CK.Taghelpers;

/// <summary>
/// Content phase: inside a tag body or at root level.
/// Add text, open child tags, or close the current tag.
/// Attributes are not available.
/// </summary>
public interface IHtmlFlow
{
    /// <summary>Opens a tag, entering the attribute phase: &lt;tag</summary>
    IHtmlTag OpenTag(string tag);

    /// <summary>Appends a complete opening tag: &lt;tag&gt; with optional class/id shorthand.</summary>
    IHtmlFlow Tag(string tag, string? cssClass = null, string? id = null);

    /// <summary>Appends a complete element: &lt;tag&gt;encoded-text&lt;/tag&gt;</summary>
    IHtmlFlow Element(string tag, string text, string? cssClass = null, string? id = null);

    /// <summary>Appends HTML-encoded text content.</summary>
    IHtmlFlow Text(string text);

    /// <summary>Conditionally appends HTML-encoded text content.</summary>
    IHtmlFlow TextIf(bool condition, string text);

    /// <summary>Appends pre-encoded or trusted HTML. Use with caution.</summary>
    IHtmlFlow Raw(string html);

    /// <summary>Appends content from an <see cref="IHtmlContent"/>.</summary>
    IHtmlFlow AppendHtml(IHtmlContent content);

    /// <summary>Appends a closing tag: &lt;/tag&gt;</summary>
    IHtmlFlow CloseTag(string tag);

    /// <summary>Opens a scoped tag that auto-closes on dispose.</summary>
    HtmlBuilder.ElementScope Scope(string tag, string? cssClass = null, string? id = null);

    /// <summary>
    /// Opens a scoped tag without closing the start bracket.
    /// Use <see cref="HtmlBuilder.TagScope.Tag"/> to add attributes,
    /// then call <see cref="IHtmlTag.CloseStart"/> before adding content.
    /// The closing tag is appended on dispose.
    /// </summary>
    HtmlBuilder.TagScope OpenScope(string tag, string? cssClass = null, string? id = null);

    /// <summary>Appends a self-closing hidden input.</summary>
    IHtmlFlow HiddenInput(string name, string value);

    /// <summary>Appends a button element with type="button".</summary>
    IHtmlFlow Button(string text, string? cssClass = null);

    /// <summary>Direct StringBuilder access for advanced scenarios. Bypasses encoding.</summary>
    IHtmlFlow Dangerous(Action<StringBuilder> action);

    /// <summary>Returns the accumulated HTML string.</summary>
    string ToHtml();

    /// <summary>Length of the accumulated content.</summary>
    int Length { get; }
}

/// <summary>
/// Attribute phase: a tag is open but the start bracket isn't closed.
/// Add attributes, then transition back to content phase
/// via <see cref="CloseStart"/> or <see cref="SelfClose"/>.
/// Text and child tags are not available.
/// </summary>
public interface IHtmlTag
{
    /// <summary>Appends an encoded attribute: name="encoded-value"</summary>
    IHtmlTag Attr(string name, string value);

    /// <summary>Appends multiple encoded attributes.</summary>
    IHtmlTag Attr(params (string Name, string Value)[] attributes);

    /// <summary>Conditionally appends an attribute.</summary>
    IHtmlTag AttrIf(bool condition, string name, string value);

    /// <summary>Appends a boolean attribute (e.g. disabled, hidden).</summary>
    IHtmlTag BoolAttr(string name);

    /// <summary>Conditionally appends a boolean attribute.</summary>
    IHtmlTag BoolAttrIf(bool condition, string name);

    /// <summary>Appends a class attribute with a base class and conditional additions.</summary>
    IHtmlTag CssClass(string baseClass, params (bool Condition, string Class)[] conditional);

    /// <summary>Closes the opening tag: &gt; - transitions to content phase.</summary>
    IHtmlFlow CloseStart();

    /// <summary>Self-closes the tag: /&gt; - transitions to content phase.</summary>
    IHtmlFlow SelfClose();
}

/// <summary>
/// A fluent, phase-safe builder for constructing HTML strings with automatic encoding.
/// <para>
/// Use <see cref="Create"/> to start a builder chain. The return types guide you
/// through valid construction sequences.
/// </para>
/// </summary>
public sealed class HtmlBuilder : IHtmlFlow, IHtmlTag, IHtmlContent
{
    private readonly StringBuilder _sb;

    private HtmlBuilder(int capacity) => _sb = new StringBuilder(capacity);

    /// <summary>Creates a new builder in the content phase.</summary>
    public static IHtmlFlow Create(int capacity = 256) => new HtmlBuilder(capacity);

    IHtmlTag IHtmlFlow.OpenTag(string tag)
    {
        _sb.Append('<').Append(tag);
        return this;
    }

    IHtmlFlow IHtmlFlow.Tag(string tag, string? cssClass, string? id)
    {
        _sb.Append('<').Append(tag);
        if (cssClass is not null)
        {
            AppendAttr("class", cssClass);
        }
        if (id is not null)
        {
            AppendAttr("id", id);
        }
        _sb.Append('>');
        return this;
    }

    IHtmlFlow IHtmlFlow.Element(string tag, string text, string? cssClass, string? id)
    {
        ((IHtmlFlow)this).Tag(tag, cssClass, id);
        _sb.Append(HtmlEncoder.Default.Encode(text));
        _sb.Append("</").Append(tag).Append('>');
        return this;
    }

    IHtmlFlow IHtmlFlow.Text(string text)
    {
        _sb.Append(HtmlEncoder.Default.Encode(text));
        return this;
    }

    IHtmlFlow IHtmlFlow.TextIf(bool condition, string text)
    {
        if (condition)
        {
            _sb.Append(HtmlEncoder.Default.Encode(text));
        }
        return this;
    }

    IHtmlFlow IHtmlFlow.Raw(string html)
    {
        _sb.Append(html);
        return this;
    }

    IHtmlFlow IHtmlFlow.AppendHtml(IHtmlContent content)
    {
        using var writer = new StringWriter(_sb);
        content.WriteTo(writer, HtmlEncoder.Default);
        return this;
    }

    IHtmlFlow IHtmlFlow.CloseTag(string tag)
    {
        _sb.Append("</").Append(tag).Append('>');
        return this;
    }

    ElementScope IHtmlFlow.Scope(string tag, string? cssClass, string? id)
    {
        ((IHtmlFlow)this).Tag(tag, cssClass, id);
        return new ElementScope(this, tag);
    }

    TagScope IHtmlFlow.OpenScope(string tag, string? cssClass, string? id)
    {
        _sb.Append('<').Append(tag);
        if (cssClass is not null)
        {
            AppendAttr("class", cssClass);
        }
        if (id is not null)
        {
            AppendAttr("id", id);
        }
        return new TagScope(this, tag);
    }

    IHtmlFlow IHtmlFlow.HiddenInput(string name, string value)
    {
        _sb.Append("<input");
        AppendAttr("type", "hidden");
        AppendAttr("name", name);
        AppendAttr("value", value);
        _sb.Append(" />");
        return this;
    }

    IHtmlFlow IHtmlFlow.Button(string text, string? cssClass)
    {
        _sb.Append("<button");
        AppendAttr("type", "button");
        if (cssClass is not null)
        {
            AppendAttr("class", cssClass);
        }
        _sb.Append('>');
        _sb.Append(HtmlEncoder.Default.Encode(text));
        _sb.Append("</button>");
        return this;
    }

    IHtmlFlow IHtmlFlow.Dangerous(Action<StringBuilder> action)
    {
        action(_sb);
        return this;
    }

    string IHtmlFlow.ToHtml() => _sb.ToString();

    int IHtmlFlow.Length => _sb.Length;

    IHtmlTag IHtmlTag.Attr(string name, string value)
    {
        AppendAttr(name, value);
        return this;
    }

    IHtmlTag IHtmlTag.Attr(params (string Name, string Value)[] attributes)
    {
        foreach (var (name, value) in attributes)
        {
            AppendAttr(name, value);
        }
        return this;
    }

    IHtmlTag IHtmlTag.AttrIf(bool condition, string name, string value)
    {
        if (condition)
        {
            AppendAttr(name, value);
        }
        return this;
    }

    IHtmlTag IHtmlTag.BoolAttr(string name)
    {
        _sb.Append(' ').Append(name);
        return this;
    }

    IHtmlTag IHtmlTag.BoolAttrIf(bool condition, string name)
    {
        if (condition)
        {
            _sb.Append(' ').Append(name);
        }
        return this;
    }

    IHtmlTag IHtmlTag.CssClass(string baseClass, params (bool Condition, string Class)[] conditional)
    {
        var combined = new StringBuilder(baseClass);
        foreach (var (condition, @class) in conditional)
        {
            if (condition)
            {
                combined.Append(' ').Append(@class);
            }
        }
        AppendAttr("class", combined.ToString());
        return this;
    }

    IHtmlFlow IHtmlTag.CloseStart()
    {
        _sb.Append('>');
        return this;
    }

    IHtmlFlow IHtmlTag.SelfClose()
    {
        _sb.Append(" />");
        return this;
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        foreach (var chunk in _sb.GetChunks())
        {
            writer.Write(chunk.Span);
        }
    }

    private void AppendAttr(string name, string value)
    {
        _sb.Append(' ').Append(name).Append("=\"")
           .Append(HtmlEncoder.Default.Encode(value))
           .Append('"');
    }

    public override string ToString() => _sb.ToString();

    /// <summary>
    /// Auto-closes a tag on dispose. The start bracket is already closed.
    /// </summary>
    public readonly struct ElementScope : IDisposable
    {
        private readonly HtmlBuilder _builder;
        private readonly string _tag;

        internal ElementScope(HtmlBuilder builder, string tag)
        {
            _builder = builder;
            _tag = tag;
        }

        public void Dispose() => _builder._sb.Append("</").Append(_tag).Append('>');
    }

    /// <summary>
    /// A scoped tag whose start bracket is still open.
    /// Use <see cref="Tag"/> to add attributes, then call
    /// <see cref="IHtmlTag.CloseStart"/> before adding content.
    /// The closing tag is appended on dispose.
    /// </summary>
    public readonly struct TagScope : IDisposable
    {
        private readonly HtmlBuilder _builder;
        private readonly string _tag;

        internal TagScope(HtmlBuilder builder, string tag)
        {
            _builder = builder;
            _tag = tag;
        }

        /// <summary>Attribute-phase handle. Add attributes, then call CloseStart().</summary>
        public IHtmlTag Tag => _builder;

        public void Dispose() => _builder._sb.Append("</").Append(_tag).Append('>');
    }
}

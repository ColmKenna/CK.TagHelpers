using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Xunit;

namespace CK.TagHelpers.Tests;

public class HtmlBuilderTests
{
    // ── Constructor tests ──────────────────────────────────────────────

    [Fact]
    public void DefaultConstructor_CreatesEmptyBuilder()
    {
        var builder = new Taghelpers.HtmlBuilder();
        Assert.Equal(string.Empty, builder.ToString());
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public void Constructor_WithCapacity_CreatesEmptyBuilder()
    {
        var builder = new Taghelpers.HtmlBuilder(512);
        Assert.Equal(string.Empty, builder.ToString());
    }

    [Fact]
    public void Constructor_WithStringBuilder_UsesProvidedBuilder()
    {
        var sb = new StringBuilder("existing");
        var builder = new Taghelpers.HtmlBuilder(sb);
        Assert.Equal("existing", builder.ToString());
        Assert.Same(sb, builder.InnerBuilder);
    }

    [Fact]
    public void Constructor_WithNullStringBuilder_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Taghelpers.HtmlBuilder(null!));
    }

    // ── OpenTag tests ──────────────────────────────────────────────────

    [Fact]
    public void OpenTag_AppendsOpenAngleBracketAndTagName()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("div")
            .ToString();

        Assert.Equal("<div", result);
    }

    [Fact]
    public void OpenTag_WithCssClass_AppendsClassAttribute()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("div", cssClass: "container")
            .CloseStart()
            .ToString();

        Assert.Equal("<div class=\"container\">", result);
    }

    [Fact]
    public void OpenTag_WithId_AppendsIdAttribute()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("div", id: "main")
            .CloseStart()
            .ToString();

        Assert.Equal("<div id=\"main\">", result);
    }

    [Fact]
    public void OpenTag_WithCssClassAndId_AppendsBothAttributes()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("div", cssClass: "container", id: "main")
            .CloseStart()
            .ToString();

        Assert.Equal("<div class=\"container\" id=\"main\">", result);
    }

    [Fact]
    public void OpenTag_WithNullCssClassAndId_AppendsNoAttributes()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("div", cssClass: null, id: null)
            .CloseStart()
            .ToString();

        Assert.Equal("<div>", result);
    }

    // ── Tag tests ──────────────────────────────────────────────────────

    [Fact]
    public void Tag_AppendsCompleteOpeningTag()
    {
        var result = new Taghelpers.HtmlBuilder()
            .Tag("div")
            .ToString();

        Assert.Equal("<div>", result);
    }

    [Fact]
    public void Tag_WithCssClass_AppendsClassAttributeAndClosesTag()
    {
        var result = new Taghelpers.HtmlBuilder()
            .Tag("div", cssClass: "container")
            .ToString();

        Assert.Equal("<div class=\"container\">", result);
    }

    [Fact]
    public void Tag_WithCssClassAndId_AppendsBothAttributesAndClosesTag()
    {
        var result = new Taghelpers.HtmlBuilder()
            .Tag("div", cssClass: "container", id: "main")
            .ToString();

        Assert.Equal("<div class=\"container\" id=\"main\">", result);
    }

    // ── Element tests ──────────────────────────────────────────────────

    [Fact]
    public void Element_AppendsCompleteElementWithEncodedText()
    {
        var result = new Taghelpers.HtmlBuilder()
            .Element("strong", "A < B & C")
            .ToString();

        Assert.Equal("<strong>A &lt; B &amp; C</strong>", result);
    }

    [Fact]
    public void Element_WithCssClassAndId_AppendsAttributesTextAndClosingTag()
    {
        var result = new Taghelpers.HtmlBuilder()
            .Element("span", "hello", cssClass: "note", id: "msg")
            .ToString();

        Assert.Equal("<span class=\"note\" id=\"msg\">hello</span>", result);
    }

    // ── Attr tests ─────────────────────────────────────────────────────

    [Fact]
    public void Attr_AppendsEncodedAttribute()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("div")
            .Attr("class", "my-class")
            .CloseStart()
            .ToString();

        Assert.Equal("<div class=\"my-class\">", result);
    }

    [Fact]
    public void Attr_EncodesSpecialCharactersInValue()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("div")
            .Attr("data-value", "a\"b<c>d&e")
            .CloseStart()
            .ToString();

        var encoded = HtmlEncoder.Default.Encode("a\"b<c>d&e");
        Assert.Equal($"<div data-value=\"{encoded}\">", result);
    }

    [Fact]
    public void Attr_MultipleAttributes_AppendsAll()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("input")
            .Attr("type", "text")
            .Attr("name", "field1")
            .Attr("value", "hello")
            .SelfClose()
            .ToString();

        Assert.Equal("<input type=\"text\" name=\"field1\" value=\"hello\" />", result);
    }

    [Fact]
    public void Attr_WithAttributePairs_AppendsAll()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("input")
            .Attr(["type", "hidden"], ["name", "field1"], ["value", "false"])
            .SelfClose()
            .ToString();

        Assert.Equal("<input type=\"hidden\" name=\"field1\" value=\"false\" />", result);
    }

    [Fact]
    public void Attr_WithInvalidAttributePair_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new Taghelpers.HtmlBuilder()
                .OpenTag("input")
                .Attr(["type"]));

        Assert.Contains("exactly two values", ex.Message);
    }

    // ── AttrIf tests ───────────────────────────────────────────────────

    [Fact]
    public void AttrIf_WhenTrue_AppendsAttribute()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("div")
            .AttrIf(true, "class", "active")
            .CloseStart()
            .ToString();

        Assert.Equal("<div class=\"active\">", result);
    }

    [Fact]
    public void AttrIf_WhenFalse_DoesNotAppendAttribute()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("div")
            .AttrIf(false, "class", "active")
            .CloseStart()
            .ToString();

        Assert.Equal("<div>", result);
    }

    // ── BoolAttr tests ─────────────────────────────────────────────────

    [Fact]
    public void BoolAttr_AppendsBooleanAttribute()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("input")
            .BoolAttr("disabled")
            .SelfClose()
            .ToString();

        Assert.Equal("<input disabled />", result);
    }

    [Fact]
    public void BoolAttrIf_WhenTrue_AppendsAttribute()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("input")
            .BoolAttrIf(true, "hidden")
            .SelfClose()
            .ToString();

        Assert.Equal("<input hidden />", result);
    }

    [Fact]
    public void BoolAttrIf_WhenFalse_DoesNotAppendAttribute()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("input")
            .BoolAttrIf(false, "hidden")
            .SelfClose()
            .ToString();

        Assert.Equal("<input />", result);
    }

    // ── CloseStart / SelfClose / CloseTag tests ────────────────────────

    [Fact]
    public void CloseStart_AppendsClosingAngleBracket()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("span")
            .CloseStart()
            .ToString();

        Assert.Equal("<span>", result);
    }

    [Fact]
    public void SelfClose_AppendsSelfClosingMarkup()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("br")
            .SelfClose()
            .ToString();

        Assert.Equal("<br />", result);
    }

    [Fact]
    public void CloseTag_AppendsFullClosingTag()
    {
        var result = new Taghelpers.HtmlBuilder()
            .CloseTag("div")
            .ToString();

        Assert.Equal("</div>", result);
    }

    // ── Text tests ─────────────────────────────────────────────────────

    [Fact]
    public void Text_AppendsEncodedContent()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("p")
            .CloseStart()
            .Text("Hello <world> & \"friends\"")
            .CloseTag("p")
            .ToString();

        var encoded = HtmlEncoder.Default.Encode("Hello <world> & \"friends\"");
        Assert.Equal($"<p>{encoded}</p>", result);
    }

    [Fact]
    public void Text_WithPlainText_AppendsUnchanged()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("span")
            .CloseStart()
            .Text("plain text")
            .CloseTag("span")
            .ToString();

        Assert.Equal("<span>plain text</span>", result);
    }

    // ── Raw tests ──────────────────────────────────────────────────────

    [Fact]
    public void Raw_AppendsWithoutEncoding()
    {
        var result = new Taghelpers.HtmlBuilder()
            .Raw("<strong>bold</strong>")
            .ToString();

        Assert.Equal("<strong>bold</strong>", result);
    }

    [Fact]
    public void Raw_DoesNotEncodeHtmlEntities()
    {
        var result = new Taghelpers.HtmlBuilder()
            .Raw("&amp; &lt;")
            .ToString();

        Assert.Equal("&amp; &lt;", result);
    }

    // ── AppendHtmlContent tests ────────────────────────────────────────

    [Fact]
    public void AppendHtmlContent_AppendsRenderedContent()
    {
        var htmlContent = new HtmlString("<em>emphasis</em>");

        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("div")
            .CloseStart()
            .AppendHtmlContent(htmlContent)
            .CloseTag("div")
            .ToString();

        Assert.Equal("<div><em>emphasis</em></div>", result);
    }

    [Fact]
    public void AppendHtmlContent_WithEmptyContent_AppendsNothing()
    {
        var htmlContent = new HtmlString(string.Empty);

        var result = new Taghelpers.HtmlBuilder()
            .AppendHtmlContent(htmlContent)
            .ToString();

        Assert.Equal(string.Empty, result);
    }

    // ── HiddenInput tests ──────────────────────────────────────────────

    [Fact]
    public void HiddenInput_GeneratesCorrectMarkup()
    {
        var result = new Taghelpers.HtmlBuilder()
            .HiddenInput("myField", "myValue")
            .ToString();

        Assert.Equal("<input type=\"hidden\" name=\"myField\" value=\"myValue\" />", result);
    }

    [Fact]
    public void HiddenInput_EncodesNameAndValue()
    {
        var result = new Taghelpers.HtmlBuilder()
            .HiddenInput("field<1>", "val\"ue")
            .ToString();

        var encodedName = HtmlEncoder.Default.Encode("field<1>");
        var encodedValue = HtmlEncoder.Default.Encode("val\"ue");
        Assert.Equal($"<input type=\"hidden\" name=\"{encodedName}\" value=\"{encodedValue}\" />", result);
    }

    // ── Button tests ───────────────────────────────────────────────────

    [Fact]
    public void Button_GeneratesCorrectMarkup()
    {
        var result = new Taghelpers.HtmlBuilder()
            .Button("btn btn-primary", "Click Me", "Click this button")
            .ToString();

        Assert.Equal(
            "<button type=\"button\" class=\"btn btn-primary\" aria-label=\"Click this button\">Click Me</button>",
            result);
    }

    [Fact]
    public void Button_WithDataAttributes_IncludesAll()
    {
        var dataAttrs = new Dictionary<string, string>
        {
            { "data-action", "delete" },
            { "data-id", "42" }
        };

        var result = new Taghelpers.HtmlBuilder()
            .Button("btn", "Delete", "Delete item", dataAttrs)
            .ToString();

        Assert.Equal(
            "<button type=\"button\" class=\"btn\" aria-label=\"Delete item\" data-action=\"delete\" data-id=\"42\">Delete</button>",
            result);
    }

    [Fact]
    public void Button_WithNullDataAttributes_OmitsThem()
    {
        var result = new Taghelpers.HtmlBuilder()
            .Button("btn", "OK", "Confirm", dataAttrs: null)
            .ToString();

        Assert.Equal(
            "<button type=\"button\" class=\"btn\" aria-label=\"Confirm\">OK</button>",
            result);
    }

    [Fact]
    public void Button_EncodesTextContent()
    {
        var result = new Taghelpers.HtmlBuilder()
            .Button("btn", "<script>alert(1)</script>", "label")
            .ToString();

        var encodedText = HtmlEncoder.Default.Encode("<script>alert(1)</script>");
        Assert.Contains(encodedText, result);
        Assert.DoesNotContain("<script>", result);
    }

    // ── Fluent chaining tests ──────────────────────────────────────────

    [Fact]
    public void FluentChaining_BuildsCompleteElement()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("div", cssClass: "wrapper", id: "root")
            .CloseStart()
            .OpenTag("p")
            .Attr("class", "text")
            .CloseStart()
            .Text("Hello World")
            .CloseTag("p")
            .CloseTag("div")
            .ToString();

        Assert.Equal(
            "<div class=\"wrapper\" id=\"root\"><p class=\"text\">Hello World</p></div>",
            result);
    }

    [Fact]
    public void FluentChaining_ReturnsSameInstance()
    {
        var builder = new Taghelpers.HtmlBuilder();

        Assert.Same(builder, builder.OpenTag("div"));
        Assert.Same(builder, builder.CloseStart());
        Assert.Same(builder, builder.Attr("a", "b"));
        Assert.Same(builder, builder.AttrIf(true, "c", "d"));
        Assert.Same(builder, builder.AttrIf(false, "e", "f"));
        Assert.Same(builder, builder.BoolAttr("disabled"));
        Assert.Same(builder, builder.BoolAttrIf(true, "hidden"));
        Assert.Same(builder, builder.BoolAttrIf(false, "readonly"));
        Assert.Same(builder, builder.Text("text"));
        Assert.Same(builder, builder.Raw("raw"));
        Assert.Same(builder, builder.SelfClose());
        Assert.Same(builder, builder.CloseTag("div"));
    }

    // ── Length / InnerBuilder tests ─────────────────────────────────────

    [Fact]
    public void Length_ReflectsAccumulatedContent()
    {
        var builder = new Taghelpers.HtmlBuilder();
        Assert.Equal(0, builder.Length);

        builder.OpenTag("p").CloseStart().Text("hi").CloseTag("p");
        Assert.Equal("<p>hi</p>".Length, builder.Length);
    }

    [Fact]
    public void InnerBuilder_AllowsDirectManipulation()
    {
        var builder = new Taghelpers.HtmlBuilder();
        builder.InnerBuilder.Append("direct");
        Assert.Equal("direct", builder.ToString());
    }

    // ── Complex scenario tests ─────────────────────────────────────────

    [Fact]
    public void ComplexScenario_MultipleHiddenInputs()
    {
        var builder = new Taghelpers.HtmlBuilder();
        builder.HiddenInput("name", "John");
        builder.HiddenInput("age", "30");

        var result = builder.ToString();
        Assert.Equal(
            "<input type=\"hidden\" name=\"name\" value=\"John\" />" +
            "<input type=\"hidden\" name=\"age\" value=\"30\" />",
            result);
    }

    [Fact]
    public void ComplexScenario_NestedElements()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("ul", cssClass: "list")
            .CloseStart()
            .OpenTag("li").CloseStart().Text("Item 1").CloseTag("li")
            .OpenTag("li").CloseStart().Text("Item 2").CloseTag("li")
            .CloseTag("ul")
            .ToString();

        Assert.Equal(
            "<ul class=\"list\"><li>Item 1</li><li>Item 2</li></ul>",
            result);
    }

    [Fact]
    public void ComplexScenario_MixedContentAndRaw()
    {
        var result = new Taghelpers.HtmlBuilder()
            .OpenTag("div").CloseStart()
            .Text("Safe: ")
            .Raw("<b>trusted</b>")
            .CloseTag("div")
            .ToString();

        Assert.Equal("<div>Safe: <b>trusted</b></div>", result);
    }
}

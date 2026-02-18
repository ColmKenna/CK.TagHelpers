using System.Text.Encodings.Web;
using CK.Taghelpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace CK.TagHelpers.Tests;

public class HtmlBuilderTests
{
    [Fact]
    public void Create_DefaultBuilder_IsEmpty()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        Assert.Equal(string.Empty, html.ToHtml());
        Assert.Equal(0, html.Length);
    }

    [Fact]
    public void Create_WithCapacity_IsEmpty()
    {
        IHtmlFlow html = HtmlBuilder.Create(512);

        Assert.Equal(string.Empty, html.ToHtml());
    }

    [Fact]
    public void OpenTag_Attr_CloseStart_BuildsTag()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.OpenTag("div")
            .Attr("class", "container")
            .Attr("id", "root")
            .CloseStart()
            .CloseTag();

        Assert.Equal("<div class=\"container\" id=\"root\"></div>", html.ToHtml());
    }

    [Fact]
    public void Attr_MultipleAttributes_AppendsAll()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.OpenTag("input")
            .Attr(("type", "hidden"), ("name", "field"), ("value", "42"))
            .SelfClose();

        Assert.Equal("<input type=\"hidden\" name=\"field\" value=\"42\" />", html.ToHtml());
    }

    [Fact]
    public void AttrIf_WhenFalse_DoesNotAppendAttribute()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.OpenTag("div")
            .AttrIf(false, "data-test", "x")
            .CloseStart();

        Assert.Equal("<div>", html.ToHtml());
    }

    [Fact]
    public void BoolAttrIf_Works()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.OpenTag("input")
            .BoolAttrIf(true, "disabled")
            .SelfClose();

        Assert.Equal("<input disabled />", html.ToHtml());
    }

    [Fact]
    public void CssClass_AppendsConditionalClasses()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.OpenTag("button")
            .CssClass("btn", (true, "btn-primary"), (false, "btn-disabled"), (true, "active"))
            .CloseStart()
            .CloseTag();

        Assert.Equal("<button class=\"btn btn-primary active\"></button>", html.ToHtml());
    }

    [Fact]
    public void Tag_AppendsCompleteStartTag()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.Tag("section", cssClass: "card", id: "main");

        Assert.Equal("<section class=\"card\" id=\"main\">", html.ToHtml());
    }

    [Fact]
    public void Element_EncodesText()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.Element("span", "A < B & C");

        Assert.Equal("<span>A &lt; B &amp; C</span>", html.ToHtml());
    }

    [Fact]
    public void TextIf_AppendsConditionally()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.TextIf(false, "skip").TextIf(true, "ok");

        Assert.Equal("ok", html.ToHtml());
    }

    [Fact]
    public void Raw_AppendsUnencodedHtml()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.Tag("div").Raw("<em>x</em>").CloseTag();

        Assert.Equal("<div><em>x</em></div>", html.ToHtml());
    }

    [Fact]
    public void AppendHtml_AppendsIHtmlContent()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.Tag("div")
            .AppendHtml(new HtmlString("<strong>ok</strong>"))
            .CloseTag();

        Assert.Equal("<div><strong>ok</strong></div>", html.ToHtml());
    }

    [Fact]
    public void HiddenInput_EncodesNameAndValue()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.HiddenInput("field<1>", "val\"ue");

        var encodedName = HtmlEncoder.Default.Encode("field<1>");
        var encodedValue = HtmlEncoder.Default.Encode("val\"ue");
        Assert.Equal($"<input type=\"hidden\" name=\"{encodedName}\" value=\"{encodedValue}\" />", html.ToHtml());
    }

    [Fact]
    public void Button_WithClass_RendersExpectedMarkup()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.Button("Click", "btn btn-primary");

        Assert.Equal("<button type=\"button\" class=\"btn btn-primary\">Click</button>", html.ToHtml());
    }

    [Fact]
    public void Button_WithoutClass_OmitsClassAttribute()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.Button("Click");

        Assert.Equal("<button type=\"button\">Click</button>", html.ToHtml());
    }

    [Fact]
    public void Dangerous_AllowsDirectStringBuilderAccess()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.Dangerous(sb => sb.Append("<!-- generated -->"));

        Assert.Equal("<!-- generated -->", html.ToHtml());
    }

    [Fact]
    public void Scope_AutoClosesTag()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        using (html.Scope("div", cssClass: "card", id: "host"))
        {
            html.Text("Body");
        }

        Assert.Equal("<div class=\"card\" id=\"host\">Body</div>", html.ToHtml());
    }

    [Fact]
    public void OpenScope_AllowsAttributesThenAutoClosesTag()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        using (var scope = html.OpenScope("form", cssClass: "editor", id: "f1"))
        {
            scope.Tag.Attr("method", "post").CloseStart();
            html.Element("label", "Name");
        }

        Assert.Equal("<form class=\"editor\" id=\"f1\" method=\"post\"><label>Name</label></form>", html.ToHtml());
    }

    [Fact]
    public void SelfClose_ClosesVoidElement()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.OpenTag("br").SelfClose();

        Assert.Equal("<br />", html.ToHtml());
    }

    [Fact]
    public void ToString_ReturnsAccumulatedHtml()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.Element("span", "hello");

        Assert.Equal("<span>hello</span>", html.ToString());
    }

    [Fact]
    public void WriteTo_WritesAllContent()
    {
        IHtmlFlow html = HtmlBuilder.Create();
        html.Element("span", "hello");

        using var writer = new StringWriter();
        ((IHtmlContent)html).WriteTo(writer, HtmlEncoder.Default);

        Assert.Equal("<span>hello</span>", writer.ToString());
    }

    [Fact]
    public void InterfacesReturnSameBuilderInstance()
    {
        IHtmlFlow flow = HtmlBuilder.Create();

        var tag = flow.OpenTag("div");
        var sameTag = tag.Attr("class", "x");
        var backToFlow = tag.CloseStart();

        Assert.Same(tag, sameTag);
        Assert.Same(flow, backToFlow);
    }

    [Fact]
    public void Builder_CanBeUsedAsIHtmlContentInTagHelperContent()
    {
        IHtmlFlow html = HtmlBuilder.Create();
        html.Element("strong", "ok");

        var content = new DefaultTagHelperContent();
        content.SetHtmlContent((IHtmlContent)html);

        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);

        Assert.Equal("<strong>ok</strong>", writer.ToString());
    }

    // ─── Tag Stack Tests ──────────────────────────────────────────────

    [Fact]
    public void CloseTag_Parameterless_PopsAndClosesCorrectTag()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.OpenDivTag()
            .Attr("class", "outer")
            .CloseStart()
            .OpenSpanTag()
            .CloseStart()
            .Text("hi")
            .CloseTag()   // closes span
            .CloseTag();  // closes div

        Assert.Equal("<div class=\"outer\"><span>hi</span></div>", html.ToHtml());
    }

    [Fact]
    public void CloseTag_OnEmptyStack_Throws()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        Assert.Throws<InvalidOperationException>(() => html.CloseTag());
    }

    [Fact]
    public void SelfClose_PopsFromStack()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.OpenInputTag()
            .Attr("type", "text")
            .SelfClose();

        // Stack should be empty — no exception when building is done
        Assert.Equal("<input type=\"text\" />", html.ToHtml());
    }

    [Fact]
    public void Tag_PushesOntoStack_CloseTag_Pops()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.UlTag()
            .LiElement("one")
            .LiElement("two")
            .CloseTag(); // closes ul

        Assert.Equal("<ul><li>one</li><li>two</li></ul>", html.ToHtml());
    }

    [Fact]
    public void Element_DoesNotAffectStack()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.DivTag()
            .StrongElement("bold")
            .SpanElement("inline")
            .CloseTag(); // closes div — Element didn't leave anything on stack

        Assert.Equal("<div><strong>bold</strong><span>inline</span></div>", html.ToHtml());
    }

    [Fact]
    public void Scope_PushesAndPopsOnDispose()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        using (html.DivScope(cssClass: "outer"))
        {
            using (html.SpanScope(cssClass: "inner"))
            {
                html.Text("nested");
            }
        }

        Assert.Equal("<div class=\"outer\"><span class=\"inner\">nested</span></div>", html.ToHtml());
    }

    [Fact]
    public void OpenScope_PushesAndPopsOnDispose()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        using (var scope = html.OpenDivScope())
        {
            scope.Tag.Attr("data-x", "1").CloseStart();
            html.Text("content");
        }

        Assert.Equal("<div data-x=\"1\">content</div>", html.ToHtml());
    }

    [Fact]
    public void NamedOpenTag_UsesStack()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.OpenButtonTag()
            .Attr("type", "submit")
            .CloseStart()
            .Text("Go")
            .CloseTag(); // stack knows it's "button"

        Assert.Equal("<button type=\"submit\">Go</button>", html.ToHtml());
    }

    [Fact]
    public void VoidElements_PushAndPop()
    {
        IHtmlFlow html = HtmlBuilder.Create();

        html.DivTag()
            .Br()
            .Hr()
            .CloseTag(); // div — Br and Hr self-closed, didn't leave stack entries

        Assert.Equal("<div><br /><hr /></div>", html.ToHtml());
    }
}

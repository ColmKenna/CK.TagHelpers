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
    /// <summary>Opens a tag, entering the attribute phase: &lt;tag. Pushes onto the tag stack.</summary>
    IHtmlTag OpenTag(string tag);

    /// <summary>Appends a complete opening tag: &lt;tag&gt; with optional class/id shorthand. Pushes onto the tag stack.</summary>
    IHtmlFlow Tag(string tag, string? cssClass = null, string? id = null);

    /// <summary>Appends a complete element: &lt;tag&gt;encoded-text&lt;/tag&gt;. Self-contained, does not affect the tag stack.</summary>
    IHtmlFlow Element(string tag, string text, string? cssClass = null, string? id = null);

    /// <summary>Appends HTML-encoded text content.</summary>
    IHtmlFlow Text(string text);

    /// <summary>Conditionally appends HTML-encoded text content.</summary>
    IHtmlFlow TextIf(bool condition, string text);

    /// <summary>Appends pre-encoded or trusted HTML. Use with caution.</summary>
    IHtmlFlow Raw(string html);

    /// <summary>Appends content from an <see cref="IHtmlContent"/>.</summary>
    IHtmlFlow AppendHtml(IHtmlContent content);

    /// <summary>
    /// Pops the most recently opened tag from the stack and appends its closing tag.
    /// Throws <see cref="InvalidOperationException"/> if no tags are open.
    /// </summary>
    IHtmlFlow CloseTag();


    /// <summary>Opens a scoped tag that auto-closes on dispose. Pushes onto the tag stack.</summary>
    HtmlBuilder.ElementScope Scope(string tag, string? cssClass = null, string? id = null);

    /// <summary>
    /// Opens a scoped tag without closing the start bracket. Pushes onto the tag stack.
    /// Use <see cref="HtmlBuilder.TagScope.Tag"/> to add attributes,
    /// then call <see cref="IHtmlTag.CloseStart"/> before adding content.
    /// The closing tag is appended on dispose.
    /// </summary>
    HtmlBuilder.TagScope OpenScope(string tag, string? cssClass = null, string? id = null);

    /// <summary>Appends a self-closing hidden input. Self-contained, does not affect the tag stack.</summary>
    IHtmlFlow HiddenInput(string name, string value);

    /// <summary>Appends a button element with type="button". Self-contained, does not affect the tag stack.</summary>
    IHtmlFlow Button(string text, string? cssClass = null);

    /// <summary>Direct StringBuilder access for advanced scenarios. Bypasses encoding.</summary>
    IHtmlFlow Dangerous(Action<StringBuilder> action);

    /// <summary>Returns the accumulated HTML string.</summary>
    string ToHtml();

    /// <summary>Length of the accumulated content.</summary>
    int Length { get; }

    // ─── Structure / Layout ───────────────────────────────────────────

    #region div
    IHtmlTag OpenDivTag() => OpenTag("div");
    IHtmlFlow DivTag(string? cssClass = null, string? id = null) => Tag("div", cssClass, id);
    IHtmlFlow DivElement(string text, string? cssClass = null, string? id = null) => Element("div", text, cssClass, id);
    HtmlBuilder.ElementScope DivScope(string? cssClass = null, string? id = null) => Scope("div", cssClass, id);
    HtmlBuilder.TagScope OpenDivScope(string? cssClass = null, string? id = null) => OpenScope("div", cssClass, id);
    #endregion

    #region span
    IHtmlTag OpenSpanTag() => OpenTag("span");
    IHtmlFlow SpanTag(string? cssClass = null, string? id = null) => Tag("span", cssClass, id);
    IHtmlFlow SpanElement(string text, string? cssClass = null, string? id = null) => Element("span", text, cssClass, id);
    HtmlBuilder.ElementScope SpanScope(string? cssClass = null, string? id = null) => Scope("span", cssClass, id);
    HtmlBuilder.TagScope OpenSpanScope(string? cssClass = null, string? id = null) => OpenScope("span", cssClass, id);
    #endregion

    #region section
    IHtmlTag OpenSectionTag() => OpenTag("section");
    IHtmlFlow SectionTag(string? cssClass = null, string? id = null) => Tag("section", cssClass, id);
    IHtmlFlow SectionElement(string text, string? cssClass = null, string? id = null) => Element("section", text, cssClass, id);
    HtmlBuilder.ElementScope SectionScope(string? cssClass = null, string? id = null) => Scope("section", cssClass, id);
    HtmlBuilder.TagScope OpenSectionScope(string? cssClass = null, string? id = null) => OpenScope("section", cssClass, id);
    #endregion

    #region article
    IHtmlTag OpenArticleTag() => OpenTag("article");
    IHtmlFlow ArticleTag(string? cssClass = null, string? id = null) => Tag("article", cssClass, id);
    IHtmlFlow ArticleElement(string text, string? cssClass = null, string? id = null) => Element("article", text, cssClass, id);
    HtmlBuilder.ElementScope ArticleScope(string? cssClass = null, string? id = null) => Scope("article", cssClass, id);
    HtmlBuilder.TagScope OpenArticleScope(string? cssClass = null, string? id = null) => OpenScope("article", cssClass, id);
    #endregion

    #region aside
    IHtmlTag OpenAsideTag() => OpenTag("aside");
    IHtmlFlow AsideTag(string? cssClass = null, string? id = null) => Tag("aside", cssClass, id);
    IHtmlFlow AsideElement(string text, string? cssClass = null, string? id = null) => Element("aside", text, cssClass, id);
    HtmlBuilder.ElementScope AsideScope(string? cssClass = null, string? id = null) => Scope("aside", cssClass, id);
    HtmlBuilder.TagScope OpenAsideScope(string? cssClass = null, string? id = null) => OpenScope("aside", cssClass, id);
    #endregion

    #region header
    IHtmlTag OpenHeaderTag() => OpenTag("header");
    IHtmlFlow HeaderTag(string? cssClass = null, string? id = null) => Tag("header", cssClass, id);
    IHtmlFlow HeaderElement(string text, string? cssClass = null, string? id = null) => Element("header", text, cssClass, id);
    HtmlBuilder.ElementScope HeaderScope(string? cssClass = null, string? id = null) => Scope("header", cssClass, id);
    HtmlBuilder.TagScope OpenHeaderScope(string? cssClass = null, string? id = null) => OpenScope("header", cssClass, id);
    #endregion

    #region footer
    IHtmlTag OpenFooterTag() => OpenTag("footer");
    IHtmlFlow FooterTag(string? cssClass = null, string? id = null) => Tag("footer", cssClass, id);
    IHtmlFlow FooterElement(string text, string? cssClass = null, string? id = null) => Element("footer", text, cssClass, id);
    HtmlBuilder.ElementScope FooterScope(string? cssClass = null, string? id = null) => Scope("footer", cssClass, id);
    HtmlBuilder.TagScope OpenFooterScope(string? cssClass = null, string? id = null) => OpenScope("footer", cssClass, id);
    #endregion

    #region main
    IHtmlTag OpenMainTag() => OpenTag("main");
    IHtmlFlow MainTag(string? cssClass = null, string? id = null) => Tag("main", cssClass, id);
    IHtmlFlow MainElement(string text, string? cssClass = null, string? id = null) => Element("main", text, cssClass, id);
    HtmlBuilder.ElementScope MainScope(string? cssClass = null, string? id = null) => Scope("main", cssClass, id);
    HtmlBuilder.TagScope OpenMainScope(string? cssClass = null, string? id = null) => OpenScope("main", cssClass, id);
    #endregion

    #region nav
    IHtmlTag OpenNavTag() => OpenTag("nav");
    IHtmlFlow NavTag(string? cssClass = null, string? id = null) => Tag("nav", cssClass, id);
    IHtmlFlow NavElement(string text, string? cssClass = null, string? id = null) => Element("nav", text, cssClass, id);
    HtmlBuilder.ElementScope NavScope(string? cssClass = null, string? id = null) => Scope("nav", cssClass, id);
    HtmlBuilder.TagScope OpenNavScope(string? cssClass = null, string? id = null) => OpenScope("nav", cssClass, id);
    #endregion

    #region figure
    IHtmlTag OpenFigureTag() => OpenTag("figure");
    IHtmlFlow FigureTag(string? cssClass = null, string? id = null) => Tag("figure", cssClass, id);
    IHtmlFlow FigureElement(string text, string? cssClass = null, string? id = null) => Element("figure", text, cssClass, id);
    HtmlBuilder.ElementScope FigureScope(string? cssClass = null, string? id = null) => Scope("figure", cssClass, id);
    HtmlBuilder.TagScope OpenFigureScope(string? cssClass = null, string? id = null) => OpenScope("figure", cssClass, id);
    #endregion

    #region figcaption
    IHtmlTag OpenFigcaptionTag() => OpenTag("figcaption");
    IHtmlFlow FigcaptionTag(string? cssClass = null, string? id = null) => Tag("figcaption", cssClass, id);
    IHtmlFlow FigcaptionElement(string text, string? cssClass = null, string? id = null) => Element("figcaption", text, cssClass, id);
    HtmlBuilder.ElementScope FigcaptionScope(string? cssClass = null, string? id = null) => Scope("figcaption", cssClass, id);
    HtmlBuilder.TagScope OpenFigcaptionScope(string? cssClass = null, string? id = null) => OpenScope("figcaption", cssClass, id);
    #endregion

    #region details
    IHtmlTag OpenDetailsTag() => OpenTag("details");
    IHtmlFlow DetailsTag(string? cssClass = null, string? id = null) => Tag("details", cssClass, id);
    HtmlBuilder.ElementScope DetailsScope(string? cssClass = null, string? id = null) => Scope("details", cssClass, id);
    HtmlBuilder.TagScope OpenDetailsScope(string? cssClass = null, string? id = null) => OpenScope("details", cssClass, id);
    #endregion

    #region summary
    IHtmlTag OpenSummaryTag() => OpenTag("summary");
    IHtmlFlow SummaryTag(string? cssClass = null, string? id = null) => Tag("summary", cssClass, id);
    IHtmlFlow SummaryElement(string text, string? cssClass = null, string? id = null) => Element("summary", text, cssClass, id);
    HtmlBuilder.ElementScope SummaryScope(string? cssClass = null, string? id = null) => Scope("summary", cssClass, id);
    HtmlBuilder.TagScope OpenSummaryScope(string? cssClass = null, string? id = null) => OpenScope("summary", cssClass, id);
    #endregion

    #region dialog
    IHtmlTag OpenDialogTag() => OpenTag("dialog");
    IHtmlFlow DialogTag(string? cssClass = null, string? id = null) => Tag("dialog", cssClass, id);
    HtmlBuilder.ElementScope DialogScope(string? cssClass = null, string? id = null) => Scope("dialog", cssClass, id);
    HtmlBuilder.TagScope OpenDialogScope(string? cssClass = null, string? id = null) => OpenScope("dialog", cssClass, id);
    #endregion

    #region template
    IHtmlTag OpenTemplateTag() => OpenTag("template");
    IHtmlFlow TemplateTag(string? cssClass = null, string? id = null) => Tag("template", cssClass, id);
    HtmlBuilder.ElementScope TemplateScope(string? cssClass = null, string? id = null) => Scope("template", cssClass, id);
    HtmlBuilder.TagScope OpenTemplateScope(string? cssClass = null, string? id = null) => OpenScope("template", cssClass, id);
    #endregion

    // ─── Text / Inline ────────────────────────────────────────────────

    #region p
    IHtmlTag OpenPTag() => OpenTag("p");
    IHtmlFlow PTag(string? cssClass = null, string? id = null) => Tag("p", cssClass, id);
    IHtmlFlow PElement(string text, string? cssClass = null, string? id = null) => Element("p", text, cssClass, id);
    HtmlBuilder.ElementScope PScope(string? cssClass = null, string? id = null) => Scope("p", cssClass, id);
    HtmlBuilder.TagScope OpenPScope(string? cssClass = null, string? id = null) => OpenScope("p", cssClass, id);
    #endregion

    #region headings
    IHtmlTag OpenH1Tag() => OpenTag("h1");
    IHtmlFlow H1Tag(string? cssClass = null, string? id = null) => Tag("h1", cssClass, id);
    IHtmlFlow H1Element(string text, string? cssClass = null, string? id = null) => Element("h1", text, cssClass, id);
    HtmlBuilder.ElementScope H1Scope(string? cssClass = null, string? id = null) => Scope("h1", cssClass, id);

    IHtmlTag OpenH2Tag() => OpenTag("h2");
    IHtmlFlow H2Tag(string? cssClass = null, string? id = null) => Tag("h2", cssClass, id);
    IHtmlFlow H2Element(string text, string? cssClass = null, string? id = null) => Element("h2", text, cssClass, id);
    HtmlBuilder.ElementScope H2Scope(string? cssClass = null, string? id = null) => Scope("h2", cssClass, id);

    IHtmlTag OpenH3Tag() => OpenTag("h3");
    IHtmlFlow H3Tag(string? cssClass = null, string? id = null) => Tag("h3", cssClass, id);
    IHtmlFlow H3Element(string text, string? cssClass = null, string? id = null) => Element("h3", text, cssClass, id);
    HtmlBuilder.ElementScope H3Scope(string? cssClass = null, string? id = null) => Scope("h3", cssClass, id);

    IHtmlTag OpenH4Tag() => OpenTag("h4");
    IHtmlFlow H4Tag(string? cssClass = null, string? id = null) => Tag("h4", cssClass, id);
    IHtmlFlow H4Element(string text, string? cssClass = null, string? id = null) => Element("h4", text, cssClass, id);
    HtmlBuilder.ElementScope H4Scope(string? cssClass = null, string? id = null) => Scope("h4", cssClass, id);

    IHtmlTag OpenH5Tag() => OpenTag("h5");
    IHtmlFlow H5Tag(string? cssClass = null, string? id = null) => Tag("h5", cssClass, id);
    IHtmlFlow H5Element(string text, string? cssClass = null, string? id = null) => Element("h5", text, cssClass, id);
    HtmlBuilder.ElementScope H5Scope(string? cssClass = null, string? id = null) => Scope("h5", cssClass, id);

    IHtmlTag OpenH6Tag() => OpenTag("h6");
    IHtmlFlow H6Tag(string? cssClass = null, string? id = null) => Tag("h6", cssClass, id);
    IHtmlFlow H6Element(string text, string? cssClass = null, string? id = null) => Element("h6", text, cssClass, id);
    HtmlBuilder.ElementScope H6Scope(string? cssClass = null, string? id = null) => Scope("h6", cssClass, id);
    #endregion

    #region strong
    IHtmlTag OpenStrongTag() => OpenTag("strong");
    IHtmlFlow StrongTag(string? cssClass = null, string? id = null) => Tag("strong", cssClass, id);
    IHtmlFlow StrongElement(string text, string? cssClass = null, string? id = null) => Element("strong", text, cssClass, id);
    HtmlBuilder.ElementScope StrongScope(string? cssClass = null, string? id = null) => Scope("strong", cssClass, id);
    #endregion

    #region em
    IHtmlTag OpenEmTag() => OpenTag("em");
    IHtmlFlow EmTag(string? cssClass = null, string? id = null) => Tag("em", cssClass, id);
    IHtmlFlow EmElement(string text, string? cssClass = null, string? id = null) => Element("em", text, cssClass, id);
    HtmlBuilder.ElementScope EmScope(string? cssClass = null, string? id = null) => Scope("em", cssClass, id);
    #endregion

    #region small
    IHtmlTag OpenSmallTag() => OpenTag("small");
    IHtmlFlow SmallTag(string? cssClass = null, string? id = null) => Tag("small", cssClass, id);
    IHtmlFlow SmallElement(string text, string? cssClass = null, string? id = null) => Element("small", text, cssClass, id);
    HtmlBuilder.ElementScope SmallScope(string? cssClass = null, string? id = null) => Scope("small", cssClass, id);
    #endregion

    #region b
    IHtmlTag OpenBTag() => OpenTag("b");
    IHtmlFlow BTag(string? cssClass = null, string? id = null) => Tag("b", cssClass, id);
    IHtmlFlow BElement(string text, string? cssClass = null, string? id = null) => Element("b", text, cssClass, id);
    HtmlBuilder.ElementScope BScope(string? cssClass = null, string? id = null) => Scope("b", cssClass, id);
    #endregion

    #region i
    IHtmlTag OpenItalicTag() => OpenTag("i");
    IHtmlFlow ItalicTag(string? cssClass = null, string? id = null) => Tag("i", cssClass, id);
    IHtmlFlow ItalicElement(string text, string? cssClass = null, string? id = null) => Element("i", text, cssClass, id);
    HtmlBuilder.ElementScope ItalicScope(string? cssClass = null, string? id = null) => Scope("i", cssClass, id);
    #endregion

    #region u
    IHtmlTag OpenUTag() => OpenTag("u");
    IHtmlFlow UTagElement(string? cssClass = null, string? id = null) => Tag("u", cssClass, id);
    IHtmlFlow UElement(string text, string? cssClass = null, string? id = null) => Element("u", text, cssClass, id);
    HtmlBuilder.ElementScope UScope(string? cssClass = null, string? id = null) => Scope("u", cssClass, id);
    #endregion

    #region s
    IHtmlTag OpenSTag() => OpenTag("s");
    IHtmlFlow STagElement(string? cssClass = null, string? id = null) => Tag("s", cssClass, id);
    IHtmlFlow SElement(string text, string? cssClass = null, string? id = null) => Element("s", text, cssClass, id);
    HtmlBuilder.ElementScope SScope(string? cssClass = null, string? id = null) => Scope("s", cssClass, id);
    #endregion

    #region code
    IHtmlTag OpenCodeTag() => OpenTag("code");
    IHtmlFlow CodeTag(string? cssClass = null, string? id = null) => Tag("code", cssClass, id);
    IHtmlFlow CodeElement(string text, string? cssClass = null, string? id = null) => Element("code", text, cssClass, id);
    HtmlBuilder.ElementScope CodeScope(string? cssClass = null, string? id = null) => Scope("code", cssClass, id);
    #endregion

    #region pre
    IHtmlTag OpenPreTag() => OpenTag("pre");
    IHtmlFlow PreTag(string? cssClass = null, string? id = null) => Tag("pre", cssClass, id);
    IHtmlFlow PreElement(string text, string? cssClass = null, string? id = null) => Element("pre", text, cssClass, id);
    HtmlBuilder.ElementScope PreScope(string? cssClass = null, string? id = null) => Scope("pre", cssClass, id);
    #endregion

    #region blockquote
    IHtmlTag OpenBlockquoteTag() => OpenTag("blockquote");
    IHtmlFlow BlockquoteTag(string? cssClass = null, string? id = null) => Tag("blockquote", cssClass, id);
    IHtmlFlow BlockquoteElement(string text, string? cssClass = null, string? id = null) => Element("blockquote", text, cssClass, id);
    HtmlBuilder.ElementScope BlockquoteScope(string? cssClass = null, string? id = null) => Scope("blockquote", cssClass, id);
    #endregion

    #region mark
    IHtmlTag OpenMarkTag() => OpenTag("mark");
    IHtmlFlow MarkTag(string? cssClass = null, string? id = null) => Tag("mark", cssClass, id);
    IHtmlFlow MarkElement(string text, string? cssClass = null, string? id = null) => Element("mark", text, cssClass, id);
    HtmlBuilder.ElementScope MarkScope(string? cssClass = null, string? id = null) => Scope("mark", cssClass, id);
    #endregion

    #region sub
    IHtmlTag OpenSubTag() => OpenTag("sub");
    IHtmlFlow SubElement(string text, string? cssClass = null, string? id = null) => Element("sub", text, cssClass, id);
    #endregion

    #region sup
    IHtmlTag OpenSupTag() => OpenTag("sup");
    IHtmlFlow SupElement(string text, string? cssClass = null, string? id = null) => Element("sup", text, cssClass, id);
    #endregion

    #region abbr
    IHtmlTag OpenAbbrTag() => OpenTag("abbr");
    IHtmlFlow AbbrElement(string text, string? cssClass = null, string? id = null) => Element("abbr", text, cssClass, id);
    #endregion

    // ─── Lists ────────────────────────────────────────────────────────

    #region ul
    IHtmlTag OpenUlTag() => OpenTag("ul");
    IHtmlFlow UlTag(string? cssClass = null, string? id = null) => Tag("ul", cssClass, id);
    HtmlBuilder.ElementScope UlScope(string? cssClass = null, string? id = null) => Scope("ul", cssClass, id);
    HtmlBuilder.TagScope OpenUlScope(string? cssClass = null, string? id = null) => OpenScope("ul", cssClass, id);
    #endregion

    #region ol
    IHtmlTag OpenOlTag() => OpenTag("ol");
    IHtmlFlow OlTag(string? cssClass = null, string? id = null) => Tag("ol", cssClass, id);
    HtmlBuilder.ElementScope OlScope(string? cssClass = null, string? id = null) => Scope("ol", cssClass, id);
    HtmlBuilder.TagScope OpenOlScope(string? cssClass = null, string? id = null) => OpenScope("ol", cssClass, id);
    #endregion

    #region li
    IHtmlTag OpenLiTag() => OpenTag("li");
    IHtmlFlow LiTag(string? cssClass = null, string? id = null) => Tag("li", cssClass, id);
    IHtmlFlow LiElement(string text, string? cssClass = null, string? id = null) => Element("li", text, cssClass, id);
    HtmlBuilder.ElementScope LiScope(string? cssClass = null, string? id = null) => Scope("li", cssClass, id);
    HtmlBuilder.TagScope OpenLiScope(string? cssClass = null, string? id = null) => OpenScope("li", cssClass, id);
    #endregion

    #region dl
    IHtmlTag OpenDlTag() => OpenTag("dl");
    IHtmlFlow DlTag(string? cssClass = null, string? id = null) => Tag("dl", cssClass, id);
    HtmlBuilder.ElementScope DlScope(string? cssClass = null, string? id = null) => Scope("dl", cssClass, id);
    #endregion

    #region dt
    IHtmlTag OpenDtTag() => OpenTag("dt");
    IHtmlFlow DtElement(string text, string? cssClass = null, string? id = null) => Element("dt", text, cssClass, id);
    #endregion

    #region dd
    IHtmlTag OpenDdTag() => OpenTag("dd");
    IHtmlFlow DdTag(string? cssClass = null, string? id = null) => Tag("dd", cssClass, id);
    IHtmlFlow DdElement(string text, string? cssClass = null, string? id = null) => Element("dd", text, cssClass, id);
    HtmlBuilder.ElementScope DdScope(string? cssClass = null, string? id = null) => Scope("dd", cssClass, id);
    #endregion

    // ─── Tables ───────────────────────────────────────────────────────

    #region table
    IHtmlTag OpenTableTag() => OpenTag("table");
    IHtmlFlow TableTag(string? cssClass = null, string? id = null) => Tag("table", cssClass, id);
    HtmlBuilder.ElementScope TableScope(string? cssClass = null, string? id = null) => Scope("table", cssClass, id);
    HtmlBuilder.TagScope OpenTableScope(string? cssClass = null, string? id = null) => OpenScope("table", cssClass, id);
    #endregion

    #region thead
    IHtmlFlow TheadTag(string? cssClass = null, string? id = null) => Tag("thead", cssClass, id);
    HtmlBuilder.ElementScope TheadScope(string? cssClass = null, string? id = null) => Scope("thead", cssClass, id);
    #endregion

    #region tbody
    IHtmlFlow TbodyTag(string? cssClass = null, string? id = null) => Tag("tbody", cssClass, id);
    HtmlBuilder.ElementScope TbodyScope(string? cssClass = null, string? id = null) => Scope("tbody", cssClass, id);
    #endregion

    #region tfoot
    IHtmlFlow TfootTag(string? cssClass = null, string? id = null) => Tag("tfoot", cssClass, id);
    HtmlBuilder.ElementScope TfootScope(string? cssClass = null, string? id = null) => Scope("tfoot", cssClass, id);
    #endregion

    #region tr
    IHtmlTag OpenTrTag() => OpenTag("tr");
    IHtmlFlow TrTag(string? cssClass = null, string? id = null) => Tag("tr", cssClass, id);
    HtmlBuilder.ElementScope TrScope(string? cssClass = null, string? id = null) => Scope("tr", cssClass, id);
    HtmlBuilder.TagScope OpenTrScope(string? cssClass = null, string? id = null) => OpenScope("tr", cssClass, id);
    #endregion

    #region th
    IHtmlTag OpenThTag() => OpenTag("th");
    IHtmlFlow ThTag(string? cssClass = null, string? id = null) => Tag("th", cssClass, id);
    IHtmlFlow ThElement(string text, string? cssClass = null, string? id = null) => Element("th", text, cssClass, id);
    HtmlBuilder.ElementScope ThScope(string? cssClass = null, string? id = null) => Scope("th", cssClass, id);
    HtmlBuilder.TagScope OpenThScope(string? cssClass = null, string? id = null) => OpenScope("th", cssClass, id);
    #endregion

    #region td
    IHtmlTag OpenTdTag() => OpenTag("td");
    IHtmlFlow TdTag(string? cssClass = null, string? id = null) => Tag("td", cssClass, id);
    IHtmlFlow TdElement(string text, string? cssClass = null, string? id = null) => Element("td", text, cssClass, id);
    HtmlBuilder.ElementScope TdScope(string? cssClass = null, string? id = null) => Scope("td", cssClass, id);
    HtmlBuilder.TagScope OpenTdScope(string? cssClass = null, string? id = null) => OpenScope("td", cssClass, id);
    #endregion

    #region caption
    IHtmlFlow CaptionElement(string text, string? cssClass = null, string? id = null) => Element("caption", text, cssClass, id);
    #endregion

    #region colgroup
    IHtmlFlow ColgroupTag(string? cssClass = null, string? id = null) => Tag("colgroup", cssClass, id);
    HtmlBuilder.ElementScope ColgroupScope(string? cssClass = null, string? id = null) => Scope("colgroup", cssClass, id);
    #endregion

    // ─── Forms ────────────────────────────────────────────────────────

    #region form
    IHtmlTag OpenFormTag() => OpenTag("form");
    IHtmlFlow FormTag(string? cssClass = null, string? id = null) => Tag("form", cssClass, id);
    HtmlBuilder.ElementScope FormScope(string? cssClass = null, string? id = null) => Scope("form", cssClass, id);
    HtmlBuilder.TagScope OpenFormScope(string? cssClass = null, string? id = null) => OpenScope("form", cssClass, id);
    #endregion

    #region input
    IHtmlTag OpenInputTag() => OpenTag("input");
    #endregion

    #region button
    IHtmlTag OpenButtonTag() => OpenTag("button");
    IHtmlFlow ButtonTag(string? cssClass = null, string? id = null) => Tag("button", cssClass, id);
    IHtmlFlow ButtonElement(string text, string? cssClass = null, string? id = null) => Element("button", text, cssClass, id);
    #endregion

    #region label
    IHtmlTag OpenLabelTag() => OpenTag("label");
    IHtmlFlow LabelTag(string? cssClass = null, string? id = null) => Tag("label", cssClass, id);
    IHtmlFlow LabelElement(string text, string? cssClass = null, string? id = null) => Element("label", text, cssClass, id);
    HtmlBuilder.ElementScope LabelScope(string? cssClass = null, string? id = null) => Scope("label", cssClass, id);
    HtmlBuilder.TagScope OpenLabelScope(string? cssClass = null, string? id = null) => OpenScope("label", cssClass, id);
    #endregion

    #region select
    IHtmlTag OpenSelectTag() => OpenTag("select");
    IHtmlFlow SelectTag(string? cssClass = null, string? id = null) => Tag("select", cssClass, id);
    HtmlBuilder.ElementScope SelectScope(string? cssClass = null, string? id = null) => Scope("select", cssClass, id);
    HtmlBuilder.TagScope OpenSelectScope(string? cssClass = null, string? id = null) => OpenScope("select", cssClass, id);
    #endregion

    #region option
    IHtmlTag OpenOptionTag() => OpenTag("option");
    IHtmlFlow OptionElement(string text, string? cssClass = null, string? id = null) => Element("option", text, cssClass, id);
    #endregion

    #region optgroup
    IHtmlTag OpenOptgroupTag() => OpenTag("optgroup");
    HtmlBuilder.ElementScope OptgroupScope(string? cssClass = null, string? id = null) => Scope("optgroup", cssClass, id);
    #endregion

    #region textarea
    IHtmlTag OpenTextareaTag() => OpenTag("textarea");
    IHtmlFlow TextareaTag(string? cssClass = null, string? id = null) => Tag("textarea", cssClass, id);
    HtmlBuilder.ElementScope TextareaScope(string? cssClass = null, string? id = null) => Scope("textarea", cssClass, id);
    HtmlBuilder.TagScope OpenTextareaScope(string? cssClass = null, string? id = null) => OpenScope("textarea", cssClass, id);
    #endregion

    #region fieldset
    IHtmlTag OpenFieldsetTag() => OpenTag("fieldset");
    IHtmlFlow FieldsetTag(string? cssClass = null, string? id = null) => Tag("fieldset", cssClass, id);
    HtmlBuilder.ElementScope FieldsetScope(string? cssClass = null, string? id = null) => Scope("fieldset", cssClass, id);
    HtmlBuilder.TagScope OpenFieldsetScope(string? cssClass = null, string? id = null) => OpenScope("fieldset", cssClass, id);
    #endregion

    #region legend
    IHtmlTag OpenLegendTag() => OpenTag("legend");
    IHtmlFlow LegendElement(string text, string? cssClass = null, string? id = null) => Element("legend", text, cssClass, id);
    #endregion

    #region output
    IHtmlTag OpenOutputTag() => OpenTag("output");
    IHtmlFlow OutputElement(string text, string? cssClass = null, string? id = null) => Element("output", text, cssClass, id);
    #endregion

    #region datalist
    IHtmlTag OpenDatalistTag() => OpenTag("datalist");
    HtmlBuilder.ElementScope DatalistScope(string? cssClass = null, string? id = null) => Scope("datalist", cssClass, id);
    #endregion

    // ─── Links / Media ────────────────────────────────────────────────

    #region a
    IHtmlTag OpenATag() => OpenTag("a");
    IHtmlFlow AElement(string text, string? cssClass = null, string? id = null) => Element("a", text, cssClass, id);
    HtmlBuilder.ElementScope AScope(string? cssClass = null, string? id = null) => Scope("a", cssClass, id);
    HtmlBuilder.TagScope OpenAScope(string? cssClass = null, string? id = null) => OpenScope("a", cssClass, id);
    #endregion

    #region img
    IHtmlTag OpenImgTag() => OpenTag("img");
    #endregion

    #region picture
    IHtmlFlow PictureTag(string? cssClass = null, string? id = null) => Tag("picture", cssClass, id);
    HtmlBuilder.ElementScope PictureScope(string? cssClass = null, string? id = null) => Scope("picture", cssClass, id);
    #endregion

    #region source
    IHtmlTag OpenSourceTag() => OpenTag("source");
    #endregion

    #region video
    IHtmlTag OpenVideoTag() => OpenTag("video");
    HtmlBuilder.ElementScope VideoScope(string? cssClass = null, string? id = null) => Scope("video", cssClass, id);
    HtmlBuilder.TagScope OpenVideoScope(string? cssClass = null, string? id = null) => OpenScope("video", cssClass, id);
    #endregion

    #region audio
    IHtmlTag OpenAudioTag() => OpenTag("audio");
    HtmlBuilder.ElementScope AudioScope(string? cssClass = null, string? id = null) => Scope("audio", cssClass, id);
    HtmlBuilder.TagScope OpenAudioScope(string? cssClass = null, string? id = null) => OpenScope("audio", cssClass, id);
    #endregion

    #region iframe
    IHtmlTag OpenIframeTag() => OpenTag("iframe");
    #endregion

    // ─── Void Elements ────────────────────────────────────────────────

    #region void elements
    IHtmlFlow Br() => OpenTag("br").SelfClose();
    IHtmlFlow Hr(string? cssClass = null) => cssClass is null
        ? OpenTag("hr").SelfClose()
        : OpenTag("hr").Attr("class", cssClass).SelfClose();
    IHtmlFlow Wbr() => OpenTag("wbr").SelfClose();
    #endregion
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

    /// <summary>Self-closes the tag: /&gt; - pops the tag from the stack and transitions to content phase.</summary>
    IHtmlFlow SelfClose();
}

/// <summary>
/// A fluent, phase-safe builder for constructing HTML strings with automatic encoding.
/// Maintains an internal tag stack to ensure correct nesting.
/// <para>
/// Use <see cref="Create"/> to start a builder chain. The return types guide you
/// through valid construction sequences.
/// </para>
/// </summary>
public sealed class HtmlBuilder : IHtmlFlow, IHtmlTag, IHtmlContent
{
    private readonly StringBuilder _sb;
    private readonly Stack<string> _tagStack = new();

    private HtmlBuilder(int capacity) => _sb = new StringBuilder(capacity);

    /// <summary>Creates a new builder in the content phase.</summary>
    public static IHtmlFlow Create(int capacity = 256) => new HtmlBuilder(capacity);

    IHtmlTag IHtmlFlow.OpenTag(string tag)
    {
        _tagStack.Push(tag);
        _sb.Append('<').Append(tag);
        return this;
    }

    IHtmlFlow IHtmlFlow.Tag(string tag, string? cssClass, string? id)
    {
        _tagStack.Push(tag);
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
        // Self-contained: writes full element without affecting the tag stack.
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

    IHtmlFlow IHtmlFlow.CloseTag()
    {
        if (_tagStack.Count == 0)
        {
            throw new InvalidOperationException("No open tags to close.");
        }
        var tag = _tagStack.Pop();
        _sb.Append("</").Append(tag).Append('>');
        return this;
    }


    ElementScope IHtmlFlow.Scope(string tag, string? cssClass, string? id)
    {
        ((IHtmlFlow)this).Tag(tag, cssClass, id); // pushes onto stack
        return new ElementScope(this, tag);
    }

    TagScope IHtmlFlow.OpenScope(string tag, string? cssClass, string? id)
    {
        _tagStack.Push(tag);
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
        if (_tagStack.Count > 0)
        {
            _tagStack.Pop();
        }
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

    /// <summary>Pops the expected tag from the stack, validates, and appends the closing tag.</summary>
    internal void PopAndCloseTag(string expectedTag)
    {
        if (_tagStack.Count == 0 || !string.Equals(_tagStack.Peek(), expectedTag, StringComparison.Ordinal))
        {
            // Scope was created with this tag, so just write the close tag.
            // Stack may be out of sync if user manually closed via CloseTag().
            _sb.Append("</").Append(expectedTag).Append('>');
            return;
        }
        _tagStack.Pop();
        _sb.Append("</").Append(expectedTag).Append('>');
    }

    public override string ToString() => _sb.ToString();

    /// <summary>
    /// Auto-closes a tag on dispose. The start bracket is already closed.
    /// Pops the tag from the builder's stack.
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

        public void Dispose() => _builder.PopAndCloseTag(_tag);
    }

    /// <summary>
    /// A scoped tag whose start bracket is still open.
    /// Use <see cref="Tag"/> to add attributes, then call
    /// <see cref="IHtmlTag.CloseStart"/> before adding content.
    /// The closing tag is appended on dispose, popping from the builder's stack.
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

        public void Dispose() => _builder.PopAndCloseTag(_tag);
    }
}

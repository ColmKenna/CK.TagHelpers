using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CK.Taghelpers.TagHelpers.FlipCard;

public class FlipCardContext
{
    /// <summary>
    /// The rendered HTML content for the front face of the card.
    /// </summary>
    public IHtmlContent? FrontContent { get; set; }

    /// <summary>
    /// The rendered HTML content for the back face of the card.
    /// </summary>
    public IHtmlContent? BackContent { get; set; }

    /// <summary>
    /// Title displayed in the front card header.
    /// </summary>
    public string FrontTitle { get; set; } = "Front";

    /// <summary>
    /// Title displayed in the back card header.
    /// </summary>
    public string BackTitle { get; set; } = "Back";
}

[HtmlTargetElement("card-front", ParentTag = "flip-card")]
[HtmlTargetElement("card-back", ParentTag = "flip-card")]
public class FlipCardFaceTagHelper : TagHelper
{
    [HtmlAttributeName("title")]
    public string Title { get; set; } = "";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.SuppressOutput();

        if (!context.Items.TryGetValue(typeof(FlipCardContext), out var item) ||
            item is not FlipCardContext cardContext)
        {
            return;
        }

        var childContent = await output.GetChildContentAsync();

        // Check which tag was used. Output.TagName can be null after SuppressOutput().
        var tagName = context.TagName ?? output.TagName;
        var isFront = string.Equals(tagName, "card-front", StringComparison.OrdinalIgnoreCase);

        if (isFront)
        {
            cardContext.FrontContent = childContent;
            cardContext.FrontTitle = string.IsNullOrEmpty(Title) ? "Front" : Title;
        }
        else
        {
            cardContext.BackContent = childContent;
            cardContext.BackTitle = string.IsNullOrEmpty(Title) ? "Back" : Title;
        }
    }
}

public class FlipCardSize
{
    /// <summary>
    /// Fixed width of the card (e.g., "300px", "20rem").
    /// Must be a valid CSS length (number + px/rem/em/%/vh/vw/vmin/vmax/ch).
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Fixed height of the card (e.g., "300px", "20rem").
    /// Must be a valid CSS length (number + px/rem/em/%/vh/vw/vmin/vmax/ch).
    /// </summary>
    public string? Height { get; set; }
}

public enum FlipDirection
{
    Horizontal,
    Vertical
}

[HtmlTargetElement("flip-card")]
public partial class FlipCardTagHelper : TagHelper
{
    [HtmlAttributeName("flip-direction")]
    public FlipDirection FlipDirection { get; set; } = FlipDirection.Horizontal;

    [HtmlAttributeName("auto-height")]
    public bool? AutoHeight { get; set; }

    [HtmlAttributeName("button-text")]
    public string ButtonText { get; set; } = "Flip";

    [HtmlAttributeName("front-button-text")]
    public string? FrontButtonText { get; set; }

    [HtmlAttributeName("back-button-text")]
    public string? BackButtonText { get; set; }

    [HtmlAttributeName("button-class")]
    public string? ButtonCssClass { get; set; }

    [HtmlAttributeName("show-buttons")]
    public bool ShowButtons { get; set; } = true;

    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Create context for child TagHelpers
        var cardContext = new FlipCardContext();
        context.Items[typeof(FlipCardContext)] = cardContext;

        // Process child content (this triggers card-front and card-back TagHelpers)
        await output.GetChildContentAsync();

        // Build CSS classes
        var flipClass = FlipDirection == FlipDirection.Vertical ? "flip-vertical" : "flip-horizontal";
        var useAutoHeight = AutoHeight ?? false;
        var cardClasses = useAutoHeight ? $"card {flipClass} auto-height" : $"card {flipClass}";

        // Container classes with flip-card scope
        var containerClasses = string.IsNullOrEmpty(CssClass)
            ? "card-container flip-card"
            : $"card-container flip-card {CssClass}";

        // Build button CSS class
        var buttonClasses = string.IsNullOrEmpty(ButtonCssClass)
            ? "rotate-button"
            : $"rotate-button {ButtonCssClass}";

        var hasBackContent = cardContext.BackContent != null;
        var renderButtons = ShowButtons && hasBackContent;

        // Build the output HTML using HtmlBuilder
        var builder = HtmlBuilder.Create();

        using (builder.DivScope(cardClasses))
        {
            // Front panel
            using (var frontScope = builder.OpenDivScope("card-front"))
            {
                frontScope.Tag.Attr("aria-hidden", "false").CloseStart();

                using (builder.DivScope("card-front-header"))
                {
                    builder.H2Element(cardContext.FrontTitle ?? "Front");

                    if (renderButtons)
                    {
                        builder.OpenButtonTag()
                            .Attr("type", "button")
                            .Attr("class", buttonClasses)
                            .BoolAttr("data-flip-card-button")
                            .Attr("aria-pressed", "false")
                            .CloseStart()
                            .Text(FrontButtonText ?? ButtonText)
                            .CloseTag();
                    }
                }

                using (builder.DivScope("card-front-content"))
                {
                    if (cardContext.FrontContent != null)
                        builder.AppendHtml(cardContext.FrontContent);
                }
            }

            // Back panel — only rendered when back content exists
            if (hasBackContent)
            {
                using (var backScope = builder.OpenDivScope("card-back"))
                {
                    backScope.Tag.Attr("aria-hidden", "true").CloseStart();

                    using (builder.DivScope("card-back-header"))
                    {
                        builder.H2Element(cardContext.BackTitle ?? "Back");

                        if (renderButtons)
                        {
                            builder.OpenButtonTag()
                                .Attr("type", "button")
                                .Attr("class", buttonClasses)
                                .BoolAttr("data-flip-card-button")
                                .Attr("aria-pressed", "false")
                                .CloseStart()
                                .Text(BackButtonText ?? ButtonText)
                                .CloseTag();
                        }
                    }

                    using (builder.DivScope("card-back-content"))
                    {
                        builder.AppendHtml(cardContext.BackContent!);
                    }
                }
            }
        }

        output.TagName = "div";
        output.Attributes.SetAttribute("class", containerClasses);
        output.Content.SetHtmlContent((IHtmlContent)builder);
    }
}

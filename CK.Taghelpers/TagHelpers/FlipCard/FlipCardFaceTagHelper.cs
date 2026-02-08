using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Drawing;
using System.Text;
using System.Text.Encodings.Web;

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

        // Read width/height from Size class

        // Determine if auto-height should be applied
        var useAutoHeight = AutoHeight ?? false;

        // Build CSS classes
        var flipClass = FlipDirection == FlipDirection.Vertical ? "flip-vertical" : "flip-horizontal";
        var autoHeightClass = useAutoHeight ? "auto-height" : "";
        var cardClasses = $"card {flipClass} {autoHeightClass}".Trim();


        // Container classes with flip-card scope
        var containerClasses = string.IsNullOrEmpty(CssClass)
            ? "card-container flip-card"
            : $"card-container flip-card {CssClass}";

        // HTML-encode titles and button text
        var encoder = HtmlEncoder.Default;
        var frontTitle = encoder.Encode(cardContext.FrontTitle ?? "Front");
        var backTitle = encoder.Encode(cardContext.BackTitle ?? "Back");
        var frontButtonText = encoder.Encode(FrontButtonText ?? ButtonText);
        var backButtonText = encoder.Encode(BackButtonText ?? ButtonText);

        // Build button CSS class
        var buttonClasses = string.IsNullOrEmpty(ButtonCssClass)
            ? "rotate-button"
            : $"rotate-button {ButtonCssClass}";

        // Convert IHtmlContent to string for template
        var frontContentHtml = GetHtmlString(cardContext.FrontContent);
        var backContentHtml = GetHtmlString(cardContext.BackContent);

        // Build the output HTML
        output.TagName = "div";
        output.Attributes.SetAttribute("class", containerClasses);

        // Check if back content exists - only show buttons and back panel if it does
        var hasBackContent = cardContext.BackContent != null;

        // Build button HTML conditionally (only if there's something to flip to)
        var frontButtonHtml = ShowButtons && hasBackContent
            ? $@"<button type=""button"" class=""{buttonClasses}"" data-flip-card-button aria-pressed=""false"">{frontButtonText}</button>"
            : "";
        var backButtonHtml = ShowButtons && hasBackContent
            ? $@"<button type=""button"" class=""{buttonClasses}"" data-flip-card-button aria-pressed=""false"">{backButtonText}</button>"
            : "";

        // Build back panel HTML only if there's back content
        var backPanelHtml = hasBackContent
            ? $@"
    <div class=""card-back"" aria-hidden=""true"">
        <div class=""card-back-header"">
            <h2>{backTitle}</h2>
            {backButtonHtml}
        </div>
        <div class=""card-back-content"">
            {backContentHtml}
        </div>
    </div>"
            : "";

        var html = $@"
<div class=""{cardClasses}"">
    <div class=""card-front"" aria-hidden=""false"">
        <div class=""card-front-header"">
            <h2>{frontTitle}</h2>
            {frontButtonHtml}
        </div>
        <div class=""card-front-content"">
            {frontContentHtml}
        </div>
    </div>{backPanelHtml}
</div>";

        output.Content.SetHtmlContent(html);
    }

    private static string GetHtmlString(IHtmlContent? content)
    {
        if (content == null) return string.Empty;
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }


}

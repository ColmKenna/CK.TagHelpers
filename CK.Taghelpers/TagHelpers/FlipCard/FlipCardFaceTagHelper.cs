using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CK.Taghelpers.TagHelpers.FlipCard;

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

        var tagName = context.TagName;
        var isFront = string.Equals(tagName, "card-front", StringComparison.OrdinalIgnoreCase);

        if (isFront)
        {
            cardContext.FrontContent = childContent;
            cardContext.FrontTitle = string.IsNullOrWhiteSpace(Title) ? "Front" : Title.Trim();
        }
        else
        {
            cardContext.BackContent = childContent;
            cardContext.BackTitle = string.IsNullOrWhiteSpace(Title) ? "Back" : Title.Trim();
        }
    }
}
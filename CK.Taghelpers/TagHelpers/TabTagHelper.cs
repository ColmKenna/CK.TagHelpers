using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CK.Taghelpers.TagHelpers;

[HtmlTargetElement("tab")]
public class TabTagHelper : TagHelper
{
    internal static readonly object UsedIdsKey = new object();
    internal static readonly object TabContextKey = new object();

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        context.Items[UsedIdsKey] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var groupName = $"tabs-{context.UniqueId}";
        context.Items[typeof(TabTagHelper)] = groupName;
        var tabContext = new TabContext();
        context.Items[TabContextKey] = tabContext;

        output.TagName = "div";
        var existingClass = output.Attributes["class"]?.Value?.ToString();
        var mergedClass = string.IsNullOrWhiteSpace(existingClass)
            ? "tabs"
            : existingClass.Contains("tabs", StringComparison.OrdinalIgnoreCase)
                ? existingClass
                : $"tabs {existingClass}";
        output.Attributes.SetAttribute("class", mergedClass);

        var childContent = await output.GetChildContentAsync();
        if (tabContext.Items.Count == 0)
        {
            output.Content.SetHtmlContent(childContent);
            return;
        }

        var hasSelection = false;
        foreach (var item in tabContext.Items)
        {
            if (item.Selected)
            {
                hasSelection = true;
                break;
            }
        }

        if (!hasSelection)
        {
            tabContext.Items[0].Selected = true;
        }

        var content = new DefaultTagHelperContent();
        foreach (var item in tabContext.Items)
        {
            if (item.Selected)
            {
                item.Input.Attributes["checked"] = "checked";
            }
            else
            {
                item.Input.Attributes.Remove("checked");
            }

            content.AppendHtml(item.Input);
            content.AppendHtml(item.Label);
            content.AppendHtml(item.Panel);
        }

        output.Content.SetHtmlContent(content);
    }
}

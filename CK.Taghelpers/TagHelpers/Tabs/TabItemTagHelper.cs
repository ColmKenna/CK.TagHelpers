using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CK.Taghelpers.TagHelpers.Tabs;

/// <summary>
/// Renders a CSS-only tab item with a heading and panel content. The <c>heading</c> attribute is required.
/// </summary>
/// <remarks>
/// If <c>id</c> is not provided, it is auto-generated from the heading text.
/// ARIA attributes (<c>role</c>, <c>aria-controls</c>, <c>aria-labelledby</c>) are included for accessibility.
/// </remarks>
[HtmlTargetElement("tab-item", ParentTag = "tab")]
public class TabItemTagHelper : TagHelper
{
    private static readonly Regex IdSanitizerRegex =
        new Regex(@"[^a-zA-Z0-9\s-]", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>Optional unique identifier for the tab. Auto-generated from heading if not provided.</summary>
    [HtmlAttributeName("id")]
    public string? Id { get; set; }

    /// <summary>When true, this tab is selected by default.</summary>
    [HtmlAttributeName("selected")]
    public bool Selected { get; set; }
    
    /// <summary>Required. The text displayed in the tab heading.</summary>
    [HtmlAttributeName("heading")]
    public string? Heading { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Heading))
        {
            output.TagName = null;
            output.Content.SetHtmlContent("<!-- TabItemTagHelper: Missing required 'heading' attribute -->");
            return;
        }

        var content = await output.GetChildContentAsync();

        if (string.IsNullOrWhiteSpace(Id))
        {
            Id = GenerateIdFromHeading(Heading);
        }

        if (string.IsNullOrWhiteSpace(Id))
        {
            Id = $"tab-{context.UniqueId}";
        }

        var usedIds = GetOrCreateUsedIds(context);
        Id = EnsureUniqueId(Id, usedIds);

        var groupName = "tabs";
        if (context.Items.TryGetValue(typeof(TabTagHelper), out var groupNameValue)
            && groupNameValue is string groupNameString
            && !string.IsNullOrWhiteSpace(groupNameString))
        {
            groupName = groupNameString;
        }

        var input = new TagBuilder("input");
        input.TagRenderMode = TagRenderMode.SelfClosing;
        input.AddCssClass("tabs-panel-input");
        input.Attributes["name"] = groupName;
        input.Attributes["type"] = "radio";
        input.Attributes["id"] = Id;
        input.Attributes["role"] = "tab";
        input.Attributes["aria-controls"] = $"{Id}-panel";
        if (Selected)
        {
            input.Attributes["checked"] = "checked";
        }

        var label = new TagBuilder("label");
        label.AddCssClass("tab-heading");
        label.Attributes["for"] = Id;
        label.InnerHtml.Append(Heading);

        var panel = new TagBuilder("div");
        panel.AddCssClass("panel");
        panel.Attributes["id"] = $"{Id}-panel";
        panel.Attributes["role"] = "tabpanel";
        panel.Attributes["aria-labelledby"] = Id;
        var panelContent = new TagBuilder("div");
        panelContent.AddCssClass("panel-content");
        panelContent.InnerHtml.AppendHtml(content);
        panel.InnerHtml.AppendHtml(panelContent);

        if (context.Items.TryGetValue(TabTagHelper.TabContextKey, out var value)
            && value is TabContext tabContext)
        {
            tabContext.Items.Add(new TabItemDescriptor(input, label, panel, Selected));
            output.SuppressOutput();
            return;
        }

        output.TagName = null; // Remove the original <tab-item> tag
        output.Content.SetHtmlContent(input);
        output.Content.AppendHtml(label);
        output.Content.AppendHtml(panel);
    }
    
    private string GenerateIdFromHeading(string heading)
    {
        // Remove invalid characters, convert to lowercase, and replace spaces with hyphens
        return IdSanitizerRegex.Replace(heading, "").ToLowerInvariant().Replace(' ', '-');
    }

    private static HashSet<string> GetOrCreateUsedIds(TagHelperContext context)
    {
        if (context.Items.TryGetValue(TabTagHelper.UsedIdsKey, out var value)
            && value is HashSet<string> usedIds)
        {
            return usedIds;
        }

        usedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        context.Items[TabTagHelper.UsedIdsKey] = usedIds;
        return usedIds;
    }

    private static string EnsureUniqueId(string baseId, HashSet<string> usedIds)
    {
        var uniqueId = baseId;
        var suffix = 1;
        while (!usedIds.Add(uniqueId))
        {
            uniqueId = $"{baseId}-{suffix}";
            suffix++;
        }

        return uniqueId;
    }
}

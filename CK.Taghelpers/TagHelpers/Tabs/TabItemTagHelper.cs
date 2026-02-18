using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
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

        var label = (IHtmlContent)HtmlBuilder.Create()
            .OpenLabelTag()
            .Attr(
                ("class", "tab-heading"),
                ("for", Id))
            .CloseStart()
            .Text(Heading)
            .CloseTag();

        var panel = (IHtmlContent)HtmlBuilder.Create()
            .OpenDivTag()
            .Attr(
                ("class", "panel"),
                ("id", $"{Id}-panel"),
                ("role", "tabpanel"),
                ("aria-labelledby", Id))
            .CloseStart()
            .DivTag(cssClass: "panel-content")
            .AppendHtml(content)
            .CloseTag()
            .CloseTag();

        if (context.Items.TryGetValue(TabTagHelper.TabContextKey, out var value)
            && value is TabContext tabContext)
        {
            tabContext.Items.Add(new TabItemDescriptor(Id, groupName, label, panel, Selected));
            output.SuppressOutput();
            return;
        }

        // Fallback: render directly (no parent TabTagHelper context)
        var html = HtmlBuilder.Create();
        html.OpenInputTag()
            .Attr(
                ("class", "tabs-panel-input"),
                ("name", groupName),
                ("type", "radio"),
                ("id", Id),
                ("role", "tab"),
                ("aria-controls", $"{Id}-panel"))
            .AttrIf(Selected, "checked", "checked")
            .SelfClose();
        html.AppendHtml(label);
        html.AppendHtml(panel);

        output.TagName = null;
        output.Content.SetHtmlContent((IHtmlContent)html);
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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CK.Taghelpers.TagHelpers;

/// <summary>
/// Renders a CSS-only tab item with a heading and panel content. The <c>heading</c> attribute is required.
/// </summary>
[HtmlTargetElement("tab-item", ParentTag = "tab")]
public class TabItemTagHelper : TagHelper
{
    private static readonly Regex IdSanitizerRegex =
        new Regex(@"[^a-z0-9\s-]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [HtmlAttributeName("id")]
    public string? Id { get; set; }

    [HtmlAttributeName("selected")]
    public bool Selected { get; set; }
    
    [HtmlAttributeName("heading")]
    public string? Heading { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(Heading))
        {
            output.SuppressOutput();
            return;
        }

        var content = await output.GetChildContentAsync();

        var idWasProvided = !string.IsNullOrWhiteSpace(Id);
        if (!idWasProvided)
        {
            Id = GenerateIdFromHeading(Heading);
        }

        if (string.IsNullOrWhiteSpace(Id))
        {
            Id = $"tab-{context.UniqueId}";
            idWasProvided = false;
        }

        var usedIds = GetOrCreateUsedIds(context);
        if (idWasProvided)
        {
            usedIds.Add(Id);
        }
        else
        {
            Id = EnsureUniqueId(Id, usedIds);
        }

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
        // Remove invalid characters and replace spaces with hyphens
        return IdSanitizerRegex.Replace(heading.ToLowerInvariant(), "").Replace(' ', '-');
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

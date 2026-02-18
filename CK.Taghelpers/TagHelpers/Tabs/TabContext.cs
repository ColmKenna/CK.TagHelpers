using Microsoft.AspNetCore.Html;

namespace CK.Taghelpers.TagHelpers.Tabs;

internal sealed class TabContext
{
    public List<TabItemDescriptor> Items { get; } = new();
}

internal sealed class TabItemDescriptor
{
    public TabItemDescriptor(string id, string groupName, IHtmlContent label, IHtmlContent panel, bool selected)
    {
        Id = id;
        GroupName = groupName;
        Label = label;
        Panel = panel;
        Selected = selected;
    }

    public string Id { get; }
    public string GroupName { get; }
    public IHtmlContent Label { get; }
    public IHtmlContent Panel { get; }
    public bool Selected { get; set; }

    public IHtmlContent BuildInput()
    {
        var html = HtmlBuilder.Create();
        html.OpenInputTag()
            .Attr(
                ("class", "tabs-panel-input"),
                ("name", GroupName),
                ("type", "radio"),
                ("id", Id),
                ("role", "tab"),
                ("aria-controls", $"{Id}-panel"))
            .AttrIf(Selected, "checked", "checked")
            .SelfClose();
        return (IHtmlContent)html;
    }
}

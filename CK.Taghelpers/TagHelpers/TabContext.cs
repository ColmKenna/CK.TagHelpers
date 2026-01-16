using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CK.Taghelpers.TagHelpers;

internal sealed class TabContext
{
    public List<TabItemDescriptor> Items { get; } = new();
}

internal sealed class TabItemDescriptor
{
    public TabItemDescriptor(TagBuilder input, TagBuilder label, TagBuilder panel, bool selected)
    {
        Input = input;
        Label = label;
        Panel = panel;
        Selected = selected;
    }

    public TagBuilder Input { get; }
    public TagBuilder Label { get; }
    public TagBuilder Panel { get; }
    public bool Selected { get; set; }
}

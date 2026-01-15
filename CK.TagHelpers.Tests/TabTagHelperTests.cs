using System.Collections.Generic;
using System.Threading.Tasks;
using CK.Taghelpers.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace CK.TagHelpers.Tests;

public class TabTagHelperTests
{
    [Fact]
    public async Task Should_SetTagNameAndClass_When_Processed()
    {
        // Arrange
        var tagHelper = new TabTagHelper();
        var context = CreateContext();
        var output = CreateOutput(
            tagName: "tab",
            childContent: "<div>Content</div>",
            attributes: new TagHelperAttributeList { { "class", "custom" } });

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("div", output.TagName);
        Assert.Equal(TagMode.StartTagAndEndTag, output.TagMode);
        Assert.Equal("tabs", output.Attributes["class"].Value);
        Assert.Contains("Content", output.Content.GetContent());
    }

    [Fact]
    public async Task Should_SelectFirstTabItem_When_NoTabItemIsChecked()
    {
        // Arrange
        var tagHelper = new TabTagHelper();
        var context = CreateContext();
        var childContent = BuildTabItemMarkup(id: "tab1", heading: "Tab 1", content: "One", selected: false)
            + BuildTabItemMarkup(id: "tab2", heading: "Tab 2", content: "Two", selected: false);
        var output = CreateOutput(tagName: "tab", childContent: childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var rendered = output.Content.GetContent();
        Assert.Contains("id=\"tab1\" checked=\"checked\"/>", rendered);
        Assert.DoesNotContain("id=\"tab2\" checked=\"checked\"/>", rendered);
        Assert.Equal(1, CountOccurrences(rendered, "checked=\"checked\""));
    }

    [Fact]
    public async Task Should_NotSelectAdditionalTabItem_When_ASelectionAlreadyExists()
    {
        // Arrange
        var tagHelper = new TabTagHelper();
        var context = CreateContext();
        var childContent = BuildTabItemMarkup(id: "tab1", heading: "Tab 1", content: "One", selected: false)
            + BuildTabItemMarkup(id: "tab2", heading: "Tab 2", content: "Two", selected: true);
        var output = CreateOutput(tagName: "tab", childContent: childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var rendered = output.Content.GetContent();
        Assert.DoesNotContain("id=\"tab1\" checked=\"checked\"/>", rendered);
        Assert.Contains("id=\"tab2\" checked=\"checked\"/>", rendered);
        Assert.Equal(1, CountOccurrences(rendered, "checked=\"checked\""));
    }

    [Fact]
    public async Task Should_LeaveContentUnchanged_When_NoTabItemInputExists()
    {
        // Arrange
        var tagHelper = new TabTagHelper();
        var context = CreateContext();
        var childContent = "<div>No tabs here</div>";
        var output = CreateOutput(tagName: "tab", childContent: childContent);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal(childContent, output.Content.GetContent());
    }

    private static TagHelperContext CreateContext(
        string tagName = "tab",
        TagHelperAttributeList? attributes = null)
    {
        return new TagHelperContext(
            tagName: tagName,
            allAttributes: attributes ?? new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "test");
    }

    private static TagHelperOutput CreateOutput(
        string tagName,
        string childContent,
        TagHelperAttributeList? attributes = null,
        TagMode tagMode = TagMode.StartTagAndEndTag)
    {
        var content = new DefaultTagHelperContent();
        content.SetHtmlContent(childContent);

        return new TagHelperOutput(
            tagName: tagName,
            attributes: attributes ?? new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
                Task.FromResult<TagHelperContent>(content))
        {
            TagMode = tagMode
        };
    }

    private static string BuildTabItemMarkup(string id, string heading, string content, bool selected)
    {
        var checkedAttribute = selected ? "checked=\"checked\"" : string.Empty;
        return $"<input class=\"tabs-panel-input\" name=\"tabs\" type=\"radio\" id=\"{id}\" {checkedAttribute}/>"
            + $"<label class=\"tab-heading\" for=\"{id}\">{heading}</label>"
            + $"<div class=\"panel\"><div class=\"panel-content\">{content}</div></div>";
    }

    private static int CountOccurrences(string content, string value)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(value))
        {
            return 0;
        }

        var count = 0;
        var index = 0;
        while ((index = content.IndexOf(value, index, System.StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}

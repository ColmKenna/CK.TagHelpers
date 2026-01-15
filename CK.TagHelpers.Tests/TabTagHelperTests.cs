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

    [Fact]
    public async Task Should_RenderDistinctRadioGroupNames_When_MultipleTabSetsAreRendered()
    {
        // Arrange
        var firstItems = new Dictionary<object, object>();
        var secondItems = new Dictionary<object, object>();
        var tagHelper = new TabTagHelper();

        var firstContext = CreateContext(items: firstItems, uniqueId: "first");
        var firstOutput = CreateOutputWithChildContent(() =>
            BuildTabItemsAsync(
                new[] { ("First", true), ("Second", false) },
                firstItems));

        var secondContext = CreateContext(items: secondItems, uniqueId: "second");
        var secondOutput = CreateOutputWithChildContent(() =>
            BuildTabItemsAsync(
                new[] { ("Third", true), ("Fourth", false) },
                secondItems));

        // Act
        await tagHelper.ProcessAsync(firstContext, firstOutput);
        await tagHelper.ProcessAsync(secondContext, secondOutput);

        // Assert
        var firstHtml = firstOutput.Content.GetContent();
        var secondHtml = secondOutput.Content.GetContent();

        Assert.Equal(2, CountOccurrences(firstHtml, "name=\"tabs-first\""));
        Assert.Equal(2, CountOccurrences(secondHtml, "name=\"tabs-second\""));
        Assert.DoesNotContain("name=\"tabs-second\"", firstHtml);
        Assert.DoesNotContain("name=\"tabs-first\"", secondHtml);
    }

    private static TagHelperContext CreateContext(
        string tagName = "tab",
        TagHelperAttributeList? attributes = null,
        IDictionary<object, object>? items = null,
        string uniqueId = "test")
    {
        return new TagHelperContext(
            tagName: tagName,
            allAttributes: attributes ?? new TagHelperAttributeList(),
            items: items ?? new Dictionary<object, object>(),
            uniqueId: uniqueId);
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

    private static TagHelperOutput CreateOutputWithChildContent(
        Func<Task<TagHelperContent>> getChildContentAsync)
    {
        return new TagHelperOutput(
            tagName: "tab",
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) => getChildContentAsync())
        {
            TagMode = TagMode.StartTagAndEndTag
        };
    }

    private static async Task<TagHelperContent> BuildTabItemsAsync(
        IReadOnlyList<(string Heading, bool Selected)> items,
        IDictionary<object, object> sharedItems)
    {
        var content = new DefaultTagHelperContent();

        for (var index = 0; index < items.Count; index++)
        {
            var item = new TabItemTagHelper
            {
                Heading = items[index].Heading,
                Selected = items[index].Selected
            };
            var itemContext = CreateContext(
                tagName: "tab-item",
                items: sharedItems,
                uniqueId: $"item-{index}");
            var itemOutput = CreateTabItemOutput(childContent: $"Body {index}");

            await item.ProcessAsync(itemContext, itemOutput);
            content.AppendHtml(itemOutput.Content);
        }

        return content;
    }

    private static TagHelperOutput CreateTabItemOutput(
        string childContent,
        TagHelperAttributeList? attributes = null)
    {
        var content = new DefaultTagHelperContent();
        content.SetHtmlContent(childContent);

        return new TagHelperOutput(
            tagName: "tab-item",
            attributes: attributes ?? new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
                Task.FromResult<TagHelperContent>(content));
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

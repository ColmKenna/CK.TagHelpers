using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CK.Taghelpers.TagHelpers.Tabs;
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
            childContent: "<div>Content</div>");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("div", output.TagName);
        Assert.Equal(TagMode.StartTagAndEndTag, output.TagMode);
        Assert.Equal("tabs", output.Attributes["class"].Value);
        Assert.Contains("Content", output.Content.GetContent());
    }

    [Fact]
    public async Task Should_MergeClass_When_CustomClassProvided()
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
        Assert.Equal("tabs custom", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task Should_NotDuplicateTabs_When_TabsClassAlreadyExists()
    {
        // Arrange
        var tagHelper = new TabTagHelper();
        var context = CreateContext();
        var output = CreateOutput(
            tagName: "tab",
            childContent: "<div>Content</div>",
            attributes: new TagHelperAttributeList { { "class", "tabs custom-class" } });

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - should preserve existing class without adding duplicate 'tabs'
        Assert.Equal("tabs custom-class", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task Should_MergeMultipleClasses_When_MultipleCustomClassesProvided()
    {
        // Arrange
        var tagHelper = new TabTagHelper();
        var context = CreateContext();
        var output = CreateOutput(
            tagName: "tab",
            childContent: "<div>Content</div>",
            attributes: new TagHelperAttributeList { { "class", "my-custom another-class" } });

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("tabs my-custom another-class", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task Should_SelectFirstTabItem_When_NoTabItemIsChecked()
    {
        // Arrange
        var tagHelper = new TabTagHelper();
        var sharedItems = new Dictionary<object, object>();
        var context = CreateContext(items: sharedItems);
        var output = CreateOutputWithChildContent(() =>
            BuildTabItemsAsync(
                new[] { ("Tab 1", false, "One"), ("Tab 2", false, "Two") },
                sharedItems));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var rendered = output.Content.GetContent();
        Assert.True(InputHasChecked(rendered, "tab-1"));
        Assert.False(InputHasChecked(rendered, "tab-2"));
        Assert.Equal(1, CountCheckedInputs(rendered));
    }

    [Fact]
    public async Task Should_SelectFirstTabItem_When_CheckedAppearsInPanelContent()
    {
        // Arrange
        var tagHelper = new TabTagHelper();
        var sharedItems = new Dictionary<object, object>();
        var context = CreateContext(items: sharedItems);
        var output = CreateOutputWithChildContent(() =>
            BuildTabItemsAsync(
                new[] { ("Tab 1", false, "<span>checked=\"checked\"</span>"), ("Tab 2", false, "Two") },
                sharedItems));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var rendered = output.Content.GetContent();
        Assert.True(InputHasChecked(rendered, "tab-1"));
        Assert.False(InputHasChecked(rendered, "tab-2"));
        Assert.Equal(1, CountCheckedInputs(rendered));
    }

    [Fact]
    public async Task Should_NotSelectAdditionalTabItem_When_ASelectionAlreadyExists()
    {
        // Arrange
        var tagHelper = new TabTagHelper();
        var sharedItems = new Dictionary<object, object>();
        var context = CreateContext(items: sharedItems);
        var output = CreateOutputWithChildContent(() =>
            BuildTabItemsAsync(
                new[] { ("Tab 1", false, "One"), ("Tab 2", true, "Two") },
                sharedItems));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var rendered = output.Content.GetContent();
        Assert.False(InputHasChecked(rendered, "tab-1"));
        Assert.True(InputHasChecked(rendered, "tab-2"));
        Assert.Equal(1, CountCheckedInputs(rendered));
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

    // --- Code-review fixes (RED phase) ---

    [Fact]
    public async Task Should_AddTabsClass_When_ExistingClassContainsTabsAsSubstring()
    {
        // Arrange
        // "my-tabs" contains the substring "tabs" but is NOT the word "tabs".
        // The container must still receive the "tabs" CSS class.
        var tagHelper = new TabTagHelper();
        var context = CreateContext();
        var output = CreateOutput(
            tagName: "tab",
            childContent: "<div>Content</div>",
            attributes: new TagHelperAttributeList { { "class", "my-tabs" } });

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert – currently FAILS: returns "my-tabs" (substring match treats it as already present)
        Assert.Equal("tabs my-tabs", output.Attributes["class"].Value);
    }

    [Fact]
    public async Task Should_SetRoleTablist_When_Processed()
    {
        // Arrange
        var tagHelper = new TabTagHelper();
        var context = CreateContext();
        var output = CreateOutput(tagName: "tab", childContent: "<div>Content</div>");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert – currently FAILS: role attribute is not set
        Assert.Equal("tablist", output.Attributes["role"]?.Value?.ToString());
    }

    // --- end code-review fixes ---

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
                new[] { ("First", true, "Body 0"), ("Second", false, "Body 1") },
                firstItems));

        var secondContext = CreateContext(items: secondItems, uniqueId: "second");
        var secondOutput = CreateOutputWithChildContent(() =>
            BuildTabItemsAsync(
                new[] { ("Third", true, "Body 2"), ("Fourth", false, "Body 3") },
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
        IReadOnlyList<(string Heading, bool Selected, string Body)> items,
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
            var itemOutput = CreateTabItemOutput(childContent: items[index].Body ?? string.Empty);

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

    private static int CountCheckedInputs(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return 0;
        }

        return Regex.Matches(
            content,
            "<input[^>]*checked=\"checked\"[^>]*>",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase).Count;
    }

    private static bool InputHasChecked(string content, string id)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(id))
        {
            return false;
        }

        var match = Regex.Match(
            content,
            $"<input[^>]*id=\"{Regex.Escape(id)}\"[^>]*>",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        return match.Success
            && match.Value.Contains("checked=\"checked\"", System.StringComparison.OrdinalIgnoreCase);
    }
}

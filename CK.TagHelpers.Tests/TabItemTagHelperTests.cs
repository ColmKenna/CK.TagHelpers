using System.Collections.Generic;
using System.Threading.Tasks;
using CK.Taghelpers.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace CK.TagHelpers.Tests;

public class TabItemTagHelperTests
{
    [Fact]
    public async Task Should_RenderMarkupWithChecked_When_SelectedIsTrueAndIdProvided()
    {
        // Arrange
        var tagHelper = new TabItemTagHelper
        {
            Id = "tab-1",
            Heading = "First",
            Selected = true
        };
        var context = CreateContext();
        var output = CreateOutput(childContent: "Body");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var rendered = output.Content.GetContent();
        Assert.Null(output.TagName);
        Assert.Contains("<input", rendered);
        Assert.Contains("class=\"tabs-panel-input\"", rendered);
        Assert.Contains("name=\"tabs\"", rendered);
        Assert.Contains("type=\"radio\"", rendered);
        Assert.Contains("id=\"tab-1\"", rendered);
        Assert.Contains("checked=\"checked\"", rendered);
        Assert.Contains("<label", rendered);
        Assert.Contains("class=\"tab-heading\"", rendered);
        Assert.Contains("for=\"tab-1\"", rendered);
        Assert.Contains(">First</label>", rendered);
        Assert.Contains("<div class=\"panel\">", rendered);
        Assert.Contains("<div class=\"panel-content\">Body</div>", rendered);
    }

    [Fact]
    public async Task Should_OmitCheckedAttribute_When_SelectedIsFalse()
    {
        // Arrange
        var tagHelper = new TabItemTagHelper
        {
            Id = "tab-1",
            Heading = "First",
            Selected = false
        };
        var context = CreateContext();
        var output = CreateOutput(childContent: "Body");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var rendered = output.Content.GetContent();
        Assert.DoesNotContain("checked=\"checked\"", rendered);
        Assert.Contains("id=\"tab-1\"", rendered);
    }

    [Theory]
    [InlineData(null, "Hello World!", "hello-world")]
    [InlineData("", "Tab 2", "tab-2")]
    public async Task Should_GenerateIdFromHeading_When_IdIsMissing(
        string? id,
        string heading,
        string expectedId)
    {
        // Arrange
        var tagHelper = new TabItemTagHelper
        {
            Heading = heading,
            Selected = false
        };
        if (id != null)
        {
            tagHelper.Id = id;
        }

        var context = CreateContext();
        var output = CreateOutput(childContent: "Body");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var rendered = output.Content.GetContent();
        Assert.Equal(expectedId, tagHelper.Id);
        Assert.Contains($"id=\"{expectedId}\"", rendered);
        Assert.Contains("class=\"tab-heading\"", rendered);
        Assert.Contains($"for=\"{expectedId}\"", rendered);
        Assert.Contains($">{heading}</label>", rendered);
    }

    [Fact]
    public async Task Should_UseProvidedId_When_IdIsSet()
    {
        // Arrange
        var tagHelper = new TabItemTagHelper
        {
            Id = "custom-id",
            Heading = "Heading Text",
            Selected = false
        };
        var context = CreateContext();
        var output = CreateOutput(childContent: "Body");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var rendered = output.Content.GetContent();
        Assert.Contains("id=\"custom-id\"", rendered);
        Assert.Contains("class=\"tab-heading\"", rendered);
        Assert.Contains("for=\"custom-id\"", rendered);
        Assert.Contains(">Heading Text</label>", rendered);
    }

    [Fact]
    public async Task Should_RenderDiagnosticComment_When_HeadingIsMissing()
    {
        // Arrange
        var tagHelper = new TabItemTagHelper();
        var context = CreateContext();
        var output = CreateOutput(childContent: "Body");

        // Act
        var exception = await Record.ExceptionAsync(() => tagHelper.ProcessAsync(context, output));

        // Assert
        Assert.Null(exception);
        Assert.Null(output.TagName);
        var content = output.Content.GetContent();
        Assert.Contains("<!-- TabItemTagHelper: Missing required 'heading' attribute -->", content);
    }

    [Fact]
    public async Task Should_EncodeHeadingAndId_When_Rendering()
    {
        // Arrange
        var tagHelper = new TabItemTagHelper
        {
            Id = "tab-\"1\"",
            Heading = "<strong>Tab</strong>",
            Selected = false
        };
        var context = CreateContext();
        var output = CreateOutput(childContent: "Body");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var rendered = output.Content.GetContent();
        Assert.Contains("id=\"tab-&quot;1&quot;\"", rendered);
        Assert.Contains("for=\"tab-&quot;1&quot;\"", rendered);
        Assert.Contains("&lt;strong&gt;Tab&lt;/strong&gt;", rendered);
        Assert.DoesNotContain("<strong>Tab</strong>", rendered);
    }

    [Fact]
    public async Task Should_UseGroupNameFromContextItems_When_Provided()
    {
        // Arrange
        var groupName = "tabs-group";
        var tagHelper = new TabItemTagHelper
        {
            Id = "tab-1",
            Heading = "First",
            Selected = false
        };
        var context = CreateContext(
            items: new Dictionary<object, object> { { typeof(TabTagHelper), groupName } });
        var output = CreateOutput(childContent: "Body");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var rendered = output.Content.GetContent();
        Assert.Contains($"name=\"{groupName}\"", rendered);
    }

    [Fact]
    public async Task Should_AppendSuffix_When_GeneratedIdAlreadyUsed()
    {
        // Arrange
        var sharedItems = new Dictionary<object, object>();
        var first = new TabItemTagHelper
        {
            Heading = "First",
            Selected = false
        };
        var second = new TabItemTagHelper
        {
            Heading = "First",
            Selected = false
        };
        var firstContext = CreateContext(items: sharedItems, uniqueId: "first");
        var secondContext = CreateContext(items: sharedItems, uniqueId: "second");
        var firstOutput = CreateOutput(childContent: "One");
        var secondOutput = CreateOutput(childContent: "Two");

        // Act
        await first.ProcessAsync(firstContext, firstOutput);
        await second.ProcessAsync(secondContext, secondOutput);

        // Assert
        Assert.Equal("first", first.Id);
        Assert.Equal("first-1", second.Id);
    }

    [Fact]
    public async Task Should_FallbackToContextUniqueId_When_GeneratedIdIsEmpty()
    {
        // Arrange
        var tagHelper = new TabItemTagHelper
        {
            Heading = "!!!",
            Selected = false
        };
        var context = CreateContext(uniqueId: "empty");
        var output = CreateOutput(childContent: "Body");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("tab-empty", tagHelper.Id);
        Assert.Contains("id=\"tab-empty\"", output.Content.GetContent());
    }

    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("HELLO WORLD", "hello-world")]
    [InlineData("HeLLo WoRLD", "hello-world")]
    [InlineData("Tab #1: Overview!", "tab-1-overview")]
    [InlineData("Section (A) & Details", "section-a--details")]
    public async Task Should_GenerateConsistentLowercaseId_When_HeadingHasMixedCase(
        string heading,
        string expectedId)
    {
        // Arrange
        var tagHelper = new TabItemTagHelper
        {
            Heading = heading,
            Selected = false
        };
        var context = CreateContext();
        var output = CreateOutput(childContent: "Body");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal(expectedId, tagHelper.Id);
        var rendered = output.Content.GetContent();
        Assert.Contains($"id=\"{expectedId}\"", rendered);
        Assert.Contains($"for=\"{expectedId}\"", rendered);
    }

    [Fact]
    public async Task Should_AppendSuffix_When_ExplicitIdAlreadyUsed()
    {
        // Arrange
        var sharedItems = new Dictionary<object, object>();
        var first = new TabItemTagHelper
        {
            Id = "my-tab",
            Heading = "First",
            Selected = false
        };
        var second = new TabItemTagHelper
        {
            Id = "my-tab", // Duplicate explicit ID
            Heading = "Second",
            Selected = false
        };
        var firstContext = CreateContext(items: sharedItems, uniqueId: "first");
        var secondContext = CreateContext(items: sharedItems, uniqueId: "second");
        var firstOutput = CreateOutput(childContent: "One");
        var secondOutput = CreateOutput(childContent: "Two");

        // Act
        await first.ProcessAsync(firstContext, firstOutput);
        await second.ProcessAsync(secondContext, secondOutput);

        // Assert - both should have unique IDs
        Assert.Equal("my-tab", first.Id);
        Assert.Equal("my-tab-1", second.Id); // Should get suffix appended
        
        var firstRendered = firstOutput.Content.GetContent();
        var secondRendered = secondOutput.Content.GetContent();
        Assert.Contains("id=\"my-tab\"", firstRendered);
        Assert.Contains("id=\"my-tab-1\"", secondRendered);
    }

    private static TagHelperContext CreateContext(
        string tagName = "tab-item",
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
}

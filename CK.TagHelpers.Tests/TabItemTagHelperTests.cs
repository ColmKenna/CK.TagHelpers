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
        Assert.Contains("<input class=\"tabs-panel-input\"", rendered);
        Assert.Contains("name=\"tabs\"", rendered);
        Assert.Contains("type=\"radio\"", rendered);
        Assert.Contains("id=\"tab-1\"", rendered);
        Assert.Contains("checked=\"checked\"", rendered);
        Assert.Contains("<label class=\"tab-heading\" for=\"tab-1\">First</label>", rendered);
        Assert.Contains("<div class=\"panel\"><div class=\"panel-content\">Body</div></div>", rendered);
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
        Assert.Contains($"<label class=\"tab-heading\" for=\"{expectedId}\">{heading}</label>", rendered);
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
        Assert.Contains("<label class=\"tab-heading\" for=\"custom-id\">Heading Text</label>", rendered);
    }

    [Fact]
    public async Task Should_SuppressOutput_When_HeadingIsMissing()
    {
        // Arrange
        var tagHelper = new TabItemTagHelper();
        var context = CreateContext();
        var output = CreateOutput(childContent: "Body");

        // Act
        var exception = await Record.ExceptionAsync(() => tagHelper.ProcessAsync(context, output));

        // Assert
        Assert.Null(exception);
        Assert.Equal(string.Empty, output.Content.GetContent());
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

    private static TagHelperContext CreateContext(
        string tagName = "tab-item",
        TagHelperAttributeList? attributes = null)
    {
        return new TagHelperContext(
            tagName: tagName,
            allAttributes: attributes ?? new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "test");
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

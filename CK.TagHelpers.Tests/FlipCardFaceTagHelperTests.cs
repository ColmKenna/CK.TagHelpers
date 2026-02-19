using System.Collections.Generic;
using System.Threading.Tasks;
using CK.Taghelpers.TagHelpers.FlipCard;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace CK.TagHelpers.Tests;

/// <summary>
/// Unit tests for <see cref="FlipCardFaceTagHelper"/>.
/// Tests the card-front and card-back child TagHelpers that set content and titles
/// on the parent FlipCardContext.
/// </summary>
public class FlipCardFaceTagHelperTests
{
    #region Helper Methods

    private static FlipCardFaceTagHelper CreateTagHelper(string? title = null)
    {
        var tagHelper = new FlipCardFaceTagHelper();
        if (title != null)
        {
            tagHelper.Title = title;
        }
        return tagHelper;
    }

    private static TagHelperContext CreateContext(
        string tagName = "card-front",
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
        string tagName = "card-front",
        string childContent = "Test Content",
        TagHelperAttributeList? attributes = null)
    {
        var content = new DefaultTagHelperContent();
        content.SetHtmlContent(childContent);

        return new TagHelperOutput(
            tagName: tagName,
            attributes: attributes ?? new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
                Task.FromResult<TagHelperContent>(content));
    }

    private static FlipCardContext CreateFlipCardContextAndAddToItems(
        IDictionary<object, object> items)
    {
        var cardContext = new FlipCardContext();
        items[typeof(FlipCardContext)] = cardContext;
        return cardContext;
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task ProcessAsync_WithCardFrontTagAndValidContext_SetsFrontContent()
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = CreateTagHelper();
        var context = CreateContext(tagName: "card-front", items: items);
        var output = CreateOutput(tagName: "card-front", childContent: "Front Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.NotNull(cardContext.FrontContent);
        Assert.Null(output.TagName); // SuppressOutput should set this to null
    }

    [Fact]
    public async Task ProcessAsync_WithCardBackTagAndValidContext_SetsBackContent()
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = CreateTagHelper();
        var context = CreateContext(tagName: "card-back", items: items);
        var output = CreateOutput(tagName: "card-back", childContent: "Back Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.NotNull(cardContext.BackContent);
        Assert.Null(output.TagName);
    }

    [Fact]
    public async Task ProcessAsync_WithBothFaces_SetsBothContents()
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        
        var frontTagHelper = CreateTagHelper(title: "My Front");
        var backTagHelper = CreateTagHelper(title: "My Back");
        
        var frontContext = CreateContext(tagName: "card-front", items: items);
        var backContext = CreateContext(tagName: "card-back", items: items);
        
        var frontOutput = CreateOutput(tagName: "card-front", childContent: "Front Data");
        var backOutput = CreateOutput(tagName: "card-back", childContent: "Back Data");

        // Act
        await frontTagHelper.ProcessAsync(frontContext, frontOutput);
        await backTagHelper.ProcessAsync(backContext, backOutput);

        // Assert
        Assert.NotNull(cardContext.FrontContent);
        Assert.NotNull(cardContext.BackContent);
        Assert.Equal("My Front", cardContext.FrontTitle);
        Assert.Equal("My Back", cardContext.BackTitle);
    }

    #endregion

    #region Title Property Tests

    [Fact]
    public async Task ProcessAsync_WithCustomTitle_SetsCustomFrontTitle()
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = CreateTagHelper(title: "Custom Front Title");
        var context = CreateContext(tagName: "card-front", items: items);
        var output = CreateOutput(tagName: "card-front", childContent: "Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("Custom Front Title", cardContext.FrontTitle);
    }

    [Fact]
    public async Task ProcessAsync_WithCustomTitle_SetsCustomBackTitle()
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = CreateTagHelper(title: "Custom Back Title");
        var context = CreateContext(tagName: "card-back", items: items);
        var output = CreateOutput(tagName: "card-back", childContent: "Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("Custom Back Title", cardContext.BackTitle);
    }

    [Theory]
    [InlineData(null, "Front")]
    [InlineData("", "Front")]
    public async Task ProcessAsync_WithNullOrEmptyTitle_UsesDefaultFrontTitle(
        string? title, string expectedTitle)
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = new FlipCardFaceTagHelper { Title = title ?? "" };
        var context = CreateContext(tagName: "card-front", items: items);
        var output = CreateOutput(tagName: "card-front", childContent: "Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal(expectedTitle, cardContext.FrontTitle);
    }

    [Theory]
    [InlineData(null, "Back")]
    [InlineData("", "Back")]
    public async Task ProcessAsync_WithNullOrEmptyTitle_UsesDefaultBackTitle(
        string? title, string expectedTitle)
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = new FlipCardFaceTagHelper { Title = title ?? "" };
        var context = CreateContext(tagName: "card-back", items: items);
        var output = CreateOutput(tagName: "card-back", childContent: "Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal(expectedTitle, cardContext.BackTitle);
    }

    [Fact]
    public async Task ProcessAsync_WithWhitespaceTitle_UsesDefaultTitle()
    {
        // Arrange - whitespace-only titles fall back to the default ("Front")
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = CreateTagHelper(title: "   ");
        var context = CreateContext(tagName: "card-front", items: items);
        var output = CreateOutput(tagName: "card-front", childContent: "Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("Front", cardContext.FrontTitle);
    }

    #endregion

    #region Context.Items State Management Tests

    [Fact]
    public async Task ProcessAsync_WithoutFlipCardContext_ReturnsEarly()
    {
        // Arrange - no FlipCardContext in items
        var tagHelper = CreateTagHelper();
        var context = CreateContext(tagName: "card-front", items: new Dictionary<object, object>());
        var output = CreateOutput(tagName: "card-front", childContent: "Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - should suppress output and return without error
        Assert.Null(output.TagName);
    }

    [Fact]
    public async Task ProcessAsync_WithWrongTypeInContextItems_ReturnsEarly()
    {
        // Arrange - wrong type stored in context items
        var items = new Dictionary<object, object>
        {
            { typeof(FlipCardContext), "not a FlipCardContext" }
        };
        var tagHelper = CreateTagHelper();
        var context = CreateContext(tagName: "card-front", items: items);
        var output = CreateOutput(tagName: "card-front", childContent: "Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - should suppress output and return without error
        Assert.Null(output.TagName);
    }

    [Fact]
    public async Task ProcessAsync_WithNullInContextItems_ReturnsEarly()
    {
        // Arrange - null stored in context items
        var items = new Dictionary<object, object>
        {
            { typeof(FlipCardContext), null! }
        };
        var tagHelper = CreateTagHelper();
        var context = CreateContext(tagName: "card-front", items: items);
        var output = CreateOutput(tagName: "card-front", childContent: "Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - should suppress output and return without error
        Assert.Null(output.TagName);
    }

    #endregion

    #region Tag Name Detection Tests

    [Fact]
    public async Task ProcessAsync_IsCaseInsensitiveForTagName()
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = CreateTagHelper(title: "Test Title");
        var context = CreateContext(tagName: "CARD-FRONT", items: items);
        var output = CreateOutput(tagName: "CARD-FRONT", childContent: "Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - should still be recognized as front
        Assert.Equal("Test Title", cardContext.FrontTitle);
        Assert.NotNull(cardContext.FrontContent);
        Assert.Null(cardContext.BackContent);
    }

    [Fact]
    public async Task ProcessAsync_WithMixedCaseCardBack_SetsBackContent()
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = CreateTagHelper(title: "Back Test");
        var context = CreateContext(tagName: "Card-Back", items: items);
        var output = CreateOutput(tagName: "Card-Back", childContent: "Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("Back Test", cardContext.BackTitle);
        Assert.NotNull(cardContext.BackContent);
        Assert.Null(cardContext.FrontContent);
    }

    [Fact]
    public async Task ProcessAsync_UsesContextTagNameWhenOutputTagNameIsNull()
    {
        // Arrange - after SuppressOutput, output.TagName may be null
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = CreateTagHelper(title: "Front Face");
        var context = CreateContext(tagName: "card-front", items: items);
        var output = CreateOutput(tagName: "card-front", childContent: "Content");
        
        // Pre-condition: output has tag name before processing
        Assert.Equal("card-front", output.TagName);

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - TagName identification should work via context.TagName
        Assert.Equal("Front Face", cardContext.FrontTitle);
        Assert.NotNull(cardContext.FrontContent);
    }

    #endregion

    #region Output Suppression Tests

    [Fact]
    public async Task ProcessAsync_AlwaysSuppressesOutput()
    {
        // Arrange
        var items = new Dictionary<object, object>();
        CreateFlipCardContextAndAddToItems(items);
        var tagHelper = CreateTagHelper();
        var context = CreateContext(tagName: "card-front", items: items);
        var output = CreateOutput(tagName: "card-front", childContent: "Content");
        output.Attributes.Add("class", "test-class");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Null(output.TagName);
    }

    [Fact]
    public async Task ProcessAsync_SuppressesOutputEvenWhenContextMissing()
    {
        // Arrange - no FlipCardContext
        var tagHelper = CreateTagHelper();
        var context = CreateContext(tagName: "card-front");
        var output = CreateOutput(tagName: "card-front", childContent: "Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - output should still be suppressed
        Assert.Null(output.TagName);
    }

    #endregion

    #region Child Content Tests

    [Fact]
    public async Task ProcessAsync_RetrievesChildContent()
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = CreateTagHelper();
        var context = CreateContext(tagName: "card-front", items: items);
        var output = CreateOutput(tagName: "card-front", childContent: "<p>Complex HTML Content</p>");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.NotNull(cardContext.FrontContent);
        // The content should be the TagHelperContent from GetChildContentAsync
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyChildContent_SetsFrontContent()
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = CreateTagHelper();
        var context = CreateContext(tagName: "card-front", items: items);
        var output = CreateOutput(tagName: "card-front", childContent: "");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - content is set even if empty
        Assert.NotNull(cardContext.FrontContent);
    }

    #endregion

    #region Default Title Tests

    [Fact]
    public async Task ProcessAsync_DefaultTitleProperty_IsEmptyString()
    {
        // Arrange
        var tagHelper = new FlipCardFaceTagHelper();

        // Assert - verify default value
        Assert.Equal("", tagHelper.Title);
    }

    #endregion

    #region FlipCardContext Class Tests

    [Fact]
    public void FlipCardContext_DefaultFrontTitle_IsFront()
    {
        // Arrange & Act
        var context = new FlipCardContext();

        // Assert
        Assert.Equal("Front", context.FrontTitle);
    }

    [Fact]
    public void FlipCardContext_DefaultBackTitle_IsBack()
    {
        // Arrange & Act
        var context = new FlipCardContext();

        // Assert
        Assert.Equal("Back", context.BackTitle);
    }

    [Fact]
    public void FlipCardContext_DefaultFrontContent_IsNull()
    {
        // Arrange & Act
        var context = new FlipCardContext();

        // Assert
        Assert.Null(context.FrontContent);
    }

    [Fact]
    public void FlipCardContext_DefaultBackContent_IsNull()
    {
        // Arrange & Act
        var context = new FlipCardContext();

        // Assert
        Assert.Null(context.BackContent);
    }

    [Fact]
    public void FlipCardContext_CanSetAllProperties()
    {
        // Arrange
        var context = new FlipCardContext();
        var frontContent = new HtmlString("Front HTML");
        var backContent = new HtmlString("Back HTML");

        // Act
        context.FrontContent = frontContent;
        context.BackContent = backContent;
        context.FrontTitle = "Custom Front";
        context.BackTitle = "Custom Back";

        // Assert
        Assert.Same(frontContent, context.FrontContent);
        Assert.Same(backContent, context.BackContent);
        Assert.Equal("Custom Front", context.FrontTitle);
        Assert.Equal("Custom Back", context.BackTitle);
    }

    #endregion

    #region Edge Case Tests

    [Theory]
    [InlineData("A very long title that contains many words and should still work correctly as a title")]
    [InlineData("Title with special chars: <>&\"'")]
    [InlineData("Unicode: 你好世界 🎉")]
    public async Task ProcessAsync_WithSpecialTitles_SetsTitleCorrectly(string title)
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        var tagHelper = CreateTagHelper(title: title);
        var context = CreateContext(tagName: "card-front", items: items);
        var output = CreateOutput(tagName: "card-front", childContent: "Content");

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal(title, cardContext.FrontTitle);
    }

    [Fact]
    public async Task ProcessAsync_SecondCallOverwritesPreviousContent()
    {
        // Arrange
        var items = new Dictionary<object, object>();
        var cardContext = CreateFlipCardContextAndAddToItems(items);
        
        var firstTagHelper = CreateTagHelper(title: "First Title");
        var secondTagHelper = CreateTagHelper(title: "Second Title");
        
        var firstContext = CreateContext(tagName: "card-front", items: items);
        var secondContext = CreateContext(tagName: "card-front", items: items);
        
        var firstOutput = CreateOutput(tagName: "card-front", childContent: "First Content");
        var secondOutput = CreateOutput(tagName: "card-front", childContent: "Second Content");

        // Act
        await firstTagHelper.ProcessAsync(firstContext, firstOutput);
        await secondTagHelper.ProcessAsync(secondContext, secondOutput);

        // Assert - second call should overwrite
        Assert.Equal("Second Title", cardContext.FrontTitle);
    }

    #endregion

    #region FlipCardTagHelper No Back Panel Tests

    /// <summary>
    /// Helper method to create FlipCardTagHelper for testing.
    /// </summary>
    private static FlipCardTagHelper CreateFlipCardTagHelper(
        bool showButtons = true,
        string buttonText = "Flip",
        FlipDirection flipDirection = FlipDirection.Horizontal,
        bool? autoHeight = null,
        string? cssClass = null,
        string? buttonCssClass = null,
        string? frontButtonText = null,
        string? backButtonText = null)
    {
        return new FlipCardTagHelper
        {
            ShowButtons = showButtons,
            ButtonText = buttonText,
            FlipDirection = flipDirection,
            AutoHeight = autoHeight,
            CssClass = cssClass,
            ButtonCssClass = buttonCssClass,
            FrontButtonText = frontButtonText,
            BackButtonText = backButtonText,
        };
    }

    /// <summary>
    /// Helper method to create a TagHelperOutput for FlipCardTagHelper that simulates
    /// child content setting the FlipCardContext values.
    /// </summary>
    private static TagHelperOutput CreateFlipCardOutput(
        IDictionary<object, object> items,
        IHtmlContent? frontContent,
        IHtmlContent? backContent,
        string frontTitle = "Front",
        string backTitle = "Back")
    {
        return new TagHelperOutput(
            tagName: "flip-card",
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
            {
                // Simulate child taghelpers setting the context values
                if (items.TryGetValue(typeof(FlipCardContext), out var item) &&
                    item is FlipCardContext cardContext)
                {
                    cardContext.FrontContent = frontContent;
                    cardContext.BackContent = backContent;
                    cardContext.FrontTitle = frontTitle;
                    cardContext.BackTitle = backTitle;
                }
                return Task.FromResult<TagHelperContent>(new DefaultTagHelperContent());
            });
    }

    [Fact]
    public async Task FlipCardTagHelper_WithNoBackContent_DoesNotRenderFrontButton()
    {
        // Arrange - Only front content, no back content
        var tagHelper = CreateFlipCardTagHelper(showButtons: true);
        var items = new Dictionary<object, object>();
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(
            items,
            frontContent: new HtmlString("<p>Front content</p>"),
            backContent: null); // No back content!

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - Should NOT contain a flip button since there's no back panel
        var outputHtml = output.Content.GetContent();
        Assert.DoesNotContain("data-flip-card-button", outputHtml);
        Assert.DoesNotContain("rotate-button", outputHtml);
    }

    [Fact]
    public async Task FlipCardTagHelper_WithNoBackContent_DoesNotRenderBackPanel()
    {
        // Arrange - Only front content
        var tagHelper = CreateFlipCardTagHelper(showButtons: true);
        var items = new Dictionary<object, object>();
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(
            items,
            frontContent: new HtmlString("<p>Front content</p>"),
            backContent: null); // No back content!

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - Should NOT contain the back panel div
        var outputHtml = output.Content.GetContent();
        Assert.DoesNotContain("card-back", outputHtml);
        Assert.DoesNotContain("card-back-header", outputHtml);
        Assert.DoesNotContain("card-back-content", outputHtml);
    }

    [Fact]
    public async Task FlipCardTagHelper_WithBackContent_RendersFrontButton()
    {
        // Arrange - Both front and back content
        var tagHelper = CreateFlipCardTagHelper(showButtons: true);
        var items = new Dictionary<object, object>();
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(
            items,
            frontContent: new HtmlString("<p>Front content</p>"),
            backContent: new HtmlString("<p>Back content</p>")); // Has back content

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - Should contain flip buttons
        var outputHtml = output.Content.GetContent();
        Assert.Contains("data-flip-card-button", outputHtml);
        Assert.Contains("rotate-button", outputHtml);
    }

    [Fact]
    public async Task FlipCardTagHelper_WithBackContentAndShowButtonsFalse_DoesNotRenderButtons()
    {
        // Arrange - Both panels but ShowButtons is false
        var tagHelper = CreateFlipCardTagHelper(showButtons: false);
        var items = new Dictionary<object, object>();
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(
            items,
            frontContent: new HtmlString("<p>Front content</p>"),
            backContent: new HtmlString("<p>Back content</p>"));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - Should NOT contain buttons (ShowButtons=false)
        var outputHtml = output.Content.GetContent();
        Assert.DoesNotContain("data-flip-card-button", outputHtml);
        // But should still have back panel
        Assert.Contains("card-back", outputHtml);
    }

    #endregion

    #region FlipCardTagHelper Direction Tests

    [Fact]
    public async Task FlipCardTagHelper_DefaultDirection_AddsFlipHorizontalClass()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper(flipDirection: FlipDirection.Horizontal);
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"));

        await tagHelper.ProcessAsync(context, output);

        Assert.Contains("flip-horizontal", output.Content.GetContent());
        Assert.DoesNotContain("flip-vertical", output.Content.GetContent());
    }

    [Fact]
    public async Task FlipCardTagHelper_WithVerticalDirection_AddsFlipVerticalClass()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper(flipDirection: FlipDirection.Vertical);
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"));

        await tagHelper.ProcessAsync(context, output);

        Assert.Contains("flip-vertical", output.Content.GetContent());
        Assert.DoesNotContain("flip-horizontal", output.Content.GetContent());
    }

    #endregion

    #region FlipCardTagHelper CssClass Tests

    [Fact]
    public async Task FlipCardTagHelper_WithCssClass_AppendsToContainerClass()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper(cssClass: "my-custom-class");
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"));

        await tagHelper.ProcessAsync(context, output);

        var containerClass = output.Attributes["class"]?.Value?.ToString();
        Assert.NotNull(containerClass);
        Assert.Contains("card-container", containerClass);
        Assert.Contains("flip-card", containerClass);
        Assert.Contains("my-custom-class", containerClass);
    }

    [Fact]
    public async Task FlipCardTagHelper_WithNoCssClass_UsesDefaultContainerClass()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper();
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"));

        await tagHelper.ProcessAsync(context, output);

        var containerClass = output.Attributes["class"]?.Value?.ToString();
        Assert.Equal("card-container flip-card", containerClass);
    }

    #endregion

    #region FlipCardTagHelper AutoHeight Tests

    [Fact]
    public async Task FlipCardTagHelper_WithAutoHeightTrue_AddsAutoHeightClass()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper(autoHeight: true);
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"));

        await tagHelper.ProcessAsync(context, output);

        Assert.Contains("auto-height", output.Content.GetContent());
    }

    [Fact]
    public async Task FlipCardTagHelper_WithAutoHeightFalse_DoesNotAddAutoHeightClass()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper(autoHeight: false);
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"));

        await tagHelper.ProcessAsync(context, output);

        Assert.DoesNotContain("auto-height", output.Content.GetContent());
    }

    #endregion

    #region FlipCardTagHelper ButtonCssClass Tests

    [Fact]
    public async Task FlipCardTagHelper_WithCustomButtonClass_UsesCustomButtonClass()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper(buttonCssClass: "btn-primary");
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"));

        await tagHelper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        Assert.Contains("rotate-button btn-primary", html);
    }

    [Fact]
    public async Task FlipCardTagHelper_WithSpecialCharsInButtonClass_HtmlEncodesInOutput()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper(buttonCssClass: "btn\" onclick=\"evil()\"");
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"));

        await tagHelper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        Assert.DoesNotContain("onclick=\"evil()\"", html);
        Assert.Contains("&quot;", html);
    }

    #endregion

    #region FlipCardTagHelper Button Text Tests

    [Fact]
    public async Task FlipCardTagHelper_WithButtonText_UsesTextForBothButtons()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper(buttonText: "Turn Over");
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"));

        await tagHelper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(html, "Turn Over").Count);
    }

    [Fact]
    public async Task FlipCardTagHelper_WithFrontAndBackButtonText_UsesSeparateTexts()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper(
            frontButtonText: "See Back",
            backButtonText: "See Front");
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"));

        await tagHelper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        Assert.Contains("See Back", html);
        Assert.Contains("See Front", html);
        Assert.DoesNotContain("Flip", html);
    }

    #endregion

    #region FlipCardTagHelper ARIA Tests

    [Fact]
    public async Task FlipCardTagHelper_FrontPanel_HasAriaHiddenFalse()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper();
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"));

        await tagHelper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        Assert.Contains(@"class=""card-front"" aria-hidden=""false""", html);
    }

    [Fact]
    public async Task FlipCardTagHelper_BackPanel_HasAriaHiddenTrue()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper();
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"));

        await tagHelper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        Assert.Contains(@"class=""card-back"" aria-hidden=""true""", html);
    }

    #endregion

    #region FlipCardTagHelper HTML Encoding Tests

    [Fact]
    public async Task FlipCardTagHelper_WithSpecialCharsInTitle_HtmlEncodesInOutput()
    {
        var items = new Dictionary<object, object>();
        var tagHelper = CreateFlipCardTagHelper();
        var context = CreateContext(tagName: "flip-card", items: items);
        var output = CreateFlipCardOutput(items,
            frontContent: new HtmlString("<p>Front</p>"),
            backContent: new HtmlString("<p>Back</p>"),
            frontTitle: "<script>alert('xss')</script>",
            backTitle: "Back & Beyond");

        await tagHelper.ProcessAsync(context, output);

        var html = output.Content.GetContent();
        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
        Assert.Contains("Back &amp; Beyond", html);
    }

    #endregion
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CK.Taghelpers.TagHelpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using Xunit;

namespace CK.TagHelpers.Tests;

public class EditArrayTagHelperTests
{
    private readonly Mock<IHtmlHelper> _htmlHelperMock;
    private readonly Mock<IViewContextAware> _viewContextAwareMock;

    public EditArrayTagHelperTests()
    {
        _htmlHelperMock = new Mock<IHtmlHelper>();
        _viewContextAwareMock = _htmlHelperMock.As<IViewContextAware>();
    }

    // ========================================================================
    // 1. Validation Tests
    // ========================================================================

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_ViewNameIsNull()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.ViewName = null!; // Force null to test validation
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            tagHelper.ProcessAsync(context, output));
        
        Assert.Contains("ViewName", exception.Message);
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_ItemsIsNull()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Items = null!; // Force null
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            tagHelper.ProcessAsync(context, output));
        
        Assert.Contains("Items", exception.Message);
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_IdIsNull()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Id = null!; // Force null
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            tagHelper.ProcessAsync(context, output));
        
        Assert.Contains("id", exception.Message);
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_DisplayModeIsTrueAndDisplayViewNameIsNull()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = null;
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            tagHelper.ProcessAsync(context, output));
        
        Assert.Contains("DisplayViewName", exception.Message);
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_ViewContextIsNull()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.ViewContext = null!; // Force null
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            tagHelper.ProcessAsync(context, output));
        
        Assert.Contains("ViewContext", exception.Message);
    }

    // ========================================================================
    // 2. Happy Path & Rendering Tests
    // ========================================================================

    [Fact]
    public async Task Should_RenderContainerDiv_When_ConfigurationIsValid()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "my-array");
        tagHelper.ContainerCssClass = "custom-container";
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Equal("div", output.TagName);
        Assert.Equal(TagMode.StartTagAndEndTag, output.TagMode);
        Assert.Contains("edit-array-my-array", output.Attributes["id"].Value.ToString());
        Assert.Contains("edit-array-container", output.Attributes["class"].Value.ToString());
        Assert.Contains("custom-container", output.Attributes["class"].Value.ToString());
    }

    [Fact]
    public async Task Should_NotDuplicateDefaultClass_When_ItemCssClassContainsDefault()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: new[] { "Item1" }, viewName: "_Editor");
        // "edit-array-item" is the default class. We provide it explicitly along with another class.
        tagHelper.ItemCssClass = "edit-array-item custom-item";
        
        SetupPartialAsync("_Editor", new HtmlString(""));
        SetupPartialAsync("_Display", new HtmlString(""));
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        // Should contain the class string exactly as provided, not "edit-array-item edit-array-item custom-item"
        Assert.Contains("class=\"edit-array-item custom-item\"", content);
        // Verify it doesn't appear twice in the same class attribute invocation (simple check)
        Assert.DoesNotContain("edit-array-item edit-array-item", content);
    }

    [Fact]
    public async Task Should_RenderReorderDataAttribute_When_EnableReorderingIsTrue()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.EnableReordering = true;
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.True(output.Attributes.ContainsName("data-reorder-enabled"));
        Assert.Equal("true", output.Attributes["data-reorder-enabled"].Value.ToString());
    }

    [Fact]
    public async Task Should_CallPartialAsyncForEachItem_When_ItemsArePresent()
    {
        // Arrange
        var items = new[] { "Item1", "Item2" };
        var tagHelper = CreateTagHelper(items: items, viewName: "_Editor");
        
        SetupPartialAsync("_Editor", new HtmlString("<span>Rendered</span>"));
        SetupPartialAsync("_Display", new HtmlString(""));
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        _htmlHelperMock.Verify(h => h.PartialAsync("_Editor", "Item1", It.IsAny<ViewDataDictionary>()), Times.Once);
        _htmlHelperMock.Verify(h => h.PartialAsync("_Editor", "Item2", It.IsAny<ViewDataDictionary>()), Times.Once);
        
        var content = output.Content.GetContent();
        Assert.Contains("edit-array-item", content);
        Assert.Contains("<span>Rendered</span>", content);
    }

    // ========================================================================
    // 3. Display Mode & Interaction Tests
    // ========================================================================

    [Fact]
    public async Task Should_RenderDisplayAndEditContainers_When_DisplayModeIsTrue()
    {
        // Arrange
        var items = new[] { "Item1" };
        var tagHelper = CreateTagHelper(items: items, viewName: "_Editor");
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "_Display";
        
        SetupPartialAsync("_Editor", new HtmlString("<input>"));
        SetupPartialAsync("_Display", new HtmlString("<label>"));
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        
        // Check display container
        Assert.Contains("class=\"display-container\"", content);
        Assert.Contains("<label>", content); // Display content
        
        // Check edit container (should be hidden initially)
        Assert.Contains("class=\"edit-container\"", content);
        Assert.Contains("style=\"display: none;\"", content);
        Assert.Contains("<input>", content); // Edit content
        
        // Check toggle buttons
        Assert.Contains("toggleEditMode", content);
        Assert.Contains("Edit", content); // Edit button text
        Assert.Contains("Done", content); // Done button text
    }

    // ========================================================================
    // 4. Template & Add Button Tests
    // ========================================================================

    public class TestItem
    {
        public string Name { get; set; }
    }

    [Fact]
    public async Task Should_RenderTemplateTag_When_RenderTemplateIsTrue()
    {
        // Arrange
        // We need a strongly typed list to test template model creation
        var items = new List<TestItem> { new TestItem() }; 
        var tagHelper = CreateTagHelper(items: Array.Empty<object>(), viewName: "_Editor"); // Empty array as placeholder in constructor
        tagHelper.Items = items; // Override with List<T> so reflection works for template model
        tagHelper.RenderTemplate = true;
        
        SetupPartialAsync("_Editor", new HtmlString("TemplateContent"));
        SetupPartialAsync("_Display", new HtmlString(""));
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("<template", content);
        Assert.Contains("</template>", content);
        Assert.Contains("__index__", content); // Check placeholder replacement logic
    }

    [Fact]
    public async Task Should_RenderAddButton_When_ShowAddButtonIsTrue()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.RenderTemplate = true;
        tagHelper.ShowAddButton = true;
        tagHelper.AddButtonText = "Add One";
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("<button", content);
        Assert.Contains("Add One", content);
        Assert.Contains("addNewItem", content);
    }

    // ========================================================================
    // 5. Placeholder & Empty State Tests
    // ========================================================================

    [Fact]
    public async Task Should_RenderPlaceholder_When_ItemsAreEmptyAndPlaceholderIsSet()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: Array.Empty<object>());
        tagHelper.EmptyPlaceholder = "No items found.";
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("edit-array-placeholder", content);
        Assert.Contains("No items found.", content);
    }

    [Fact]
    public async Task Should_NotRenderPlaceholderDiv_When_ItemsAreEmptyAndPlaceholderIsNotSet()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: Array.Empty<object>());
        tagHelper.EmptyPlaceholder = null;
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.DoesNotContain("edit-array-placeholder", content);
    }

    // ========================================================================
    // 6. Callback Tests
    // ========================================================================

    [Fact]
    public async Task Should_RenderOnclickHandlers_When_CallbacksAreSet()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: new[] { "Item1" }, viewName: "_Editor");
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = "_Display";
        tagHelper.OnUpdate = "myUpdateCallback";
        tagHelper.OnDelete = "myDeleteCallback";
        
        SetupPartialAsync("_Editor", new HtmlString(""));
        SetupPartialAsync("_Display", new HtmlString(""));
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        
        // Verify data attributes are present (XSS-safe approach)
        Assert.Contains("data-on-update=\"myUpdateCallback\"", content);
        Assert.Contains("data-on-delete=\"myDeleteCallback\"", content);
        
        // Verify core button handlers are still present
        Assert.Contains("markForDeletion", content);
        Assert.Contains("toggleEditMode", content);
    }

    // ========================================================================
    // 7. Styling & Reordering Tests
    // ========================================================================

    [Fact]
    public async Task Should_RenderReorderButtonsInTemplate_When_RenderTemplateAndReorderingAreTrue()
    {
        // Arrange
        var items = new List<TestItem> { new TestItem() };
        var tagHelper = CreateTagHelper(items: Array.Empty<object>(), viewName: "_Editor");
        tagHelper.Items = items;
        tagHelper.RenderTemplate = true;
        tagHelper.EnableReordering = true;
        
        SetupPartialAsync("_Editor", new HtmlString("Template content"));
        SetupPartialAsync("_Display", new HtmlString(""));
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        
        // Extract template content to verify reorder buttons are inside it
        var templateStart = content.IndexOf("<template", StringComparison.Ordinal);
        var templateEnd = content.IndexOf("</template>", StringComparison.Ordinal);
        var templateContent = content.Substring(templateStart, templateEnd - templateStart);

        Assert.Contains("reorder-controls", templateContent);
        Assert.Contains("reorder-up-btn", templateContent);
        Assert.Contains("reorder-down-btn", templateContent);
        // Template reorder buttons use a different ID selector logic (this.closest)
        Assert.Contains("this.closest('.edit-array-item').id", templateContent);
    }

    [Fact]
    public async Task Should_RenderMoveButtons_When_EnableReorderingIsTrue()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: new[] { "Item1" }, viewName: "_Editor");
        tagHelper.EnableReordering = true;
        tagHelper.MoveUpButtonText = "Go Up";
        tagHelper.MoveDownButtonText = "Go Down";
        
        SetupPartialAsync("_Editor", new HtmlString(""));
        SetupPartialAsync("_Display", new HtmlString(""));
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("reorder-up-btn", content);
        Assert.Contains("reorder-down-btn", content);
        Assert.Contains("Go Up", content);
        Assert.Contains("Go Down", content);
        Assert.Contains("moveItem", content);
    }

    // ========================================================================
    // Helper Methods
    // ========================================================================

    private EditArrayTagHelper CreateTagHelper(
        object[] items = null,
        string viewName = "_ItemEditor",
        string id = "test-array")
    {
        var tagHelper = new EditArrayTagHelper(_htmlHelperMock.Object)
        {
            Items = items ?? Array.Empty<object>(),
            ViewName = viewName,
            Id = id,
            DisplayViewName = "_Display",
            // Create a fresh ViewContext for each test to avoid state pollution
            ViewContext = CreateViewContext()
        };
        return tagHelper;
    }

    private ViewContext CreateViewContext()
    {
        var actionContext = new ActionContext(
            new Microsoft.AspNetCore.Http.DefaultHttpContext(),
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
        viewData.TemplateInfo.HtmlFieldPrefix = "TestPrefix";

        return new ViewContext(
            actionContext,
            new Mock<IView>().Object,
            viewData,
            new TempDataDictionary(actionContext.HttpContext, Mock.Of<ITempDataProvider>()),
            TextWriter.Null,
            new HtmlHelperOptions());
    }

    private static TagHelperContext CreateContext(
        string tagName = "edit-array",
        TagHelperAttributeList attributes = null)
    {
        return new TagHelperContext(
            tagName: tagName,
            allAttributes: attributes ?? new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "test");
    }

    private static TagHelperOutput CreateOutput(
        string tagName = "edit-array")
    {
        return new TagHelperOutput(
            tagName: tagName,
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCached, encoder) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }

    private void SetupPartialAsync(string viewName, IHtmlContent content)
    {
        _htmlHelperMock
            .Setup(h => h.PartialAsync(viewName, It.IsAny<object>(), It.IsAny<ViewDataDictionary>()))
            .ReturnsAsync(content);
    }
}

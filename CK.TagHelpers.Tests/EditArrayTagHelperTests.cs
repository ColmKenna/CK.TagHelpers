using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CK.Taghelpers.TagHelpers.EditArray;
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
    // 1. Validation Tests (Graceful Degradation)
    // ========================================================================

    [Fact]
    public async Task Should_RenderErrorMessage_When_ViewNameIsNull()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.ViewName = null!; // Force null to test validation
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert - should render error panel instead of throwing
        Assert.Equal("div", output.TagName);
        Assert.Contains("edit-array-error", output.Attributes["class"].Value.ToString());
        Assert.Contains("alert-danger", output.Attributes["class"].Value.ToString());
        
        var content = output.Content.GetContent();
        Assert.Contains("EditArrayTagHelper Configuration Error", content);
        Assert.Contains("ViewName", content);
    }

    [Fact]
    public async Task Should_RenderErrorMessage_When_ItemsIsNull()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.Items = null!; // Force null
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert - should render error panel instead of throwing
        Assert.Equal("div", output.TagName);
        Assert.Contains("edit-array-error", output.Attributes["class"].Value.ToString());
        
        var content = output.Content.GetContent();
        Assert.Contains("EditArrayTagHelper Configuration Error", content);
        Assert.Contains("Items", content);
    }

    [Fact]
    public async Task Should_RenderErrorMessage_When_IdIsNull()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.ArrayId = null!; // Force null
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert - should render error panel instead of throwing
        Assert.Equal("div", output.TagName);
        Assert.Contains("edit-array-error", output.Attributes["class"].Value.ToString());
        
        var content = output.Content.GetContent();
        Assert.Contains("EditArrayTagHelper Configuration Error", content);
        Assert.Contains("id", content.ToLowerInvariant());
    }

    [Fact]
    public async Task Should_RenderErrorMessage_When_DisplayModeIsTrueAndDisplayViewNameIsNull()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.DisplayMode = true;
        tagHelper.DisplayViewName = null;
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert - should render error panel instead of throwing
        Assert.Equal("div", output.TagName);
        Assert.Contains("edit-array-error", output.Attributes["class"].Value.ToString());
        
        var content = output.Content.GetContent();
        Assert.Contains("EditArrayTagHelper Configuration Error", content);
        Assert.Contains("DisplayViewName", content);
    }

    [Fact]
    public async Task Should_RenderErrorMessage_When_MaxItemsIsZero()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.MaxItems = 0;

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert - should render error panel instead of throwing
        Assert.Equal("div", output.TagName);
        Assert.Contains("edit-array-error", output.Attributes["class"].Value.ToString());

        var content = output.Content.GetContent();
        Assert.Contains("MaxItems", content);
        Assert.Contains("greater than 0", content);
    }

    [Fact]
    public async Task Should_RenderErrorMessage_When_ViewContextIsNull()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.ViewContext = null!; // Force null
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert - should render error panel instead of throwing
        Assert.Equal("div", output.TagName);
        Assert.Contains("edit-array-error", output.Attributes["class"].Value.ToString());
        
        var content = output.Content.GetContent();
        Assert.Contains("EditArrayTagHelper Configuration Error", content);
        Assert.Contains("ViewContext", content);
    }

    [Fact]
    public async Task Should_RenderAllErrors_When_MultipleValidationErrorsExist()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.ViewName = null!;
        tagHelper.Items = null!;
        tagHelper.ArrayId = null!;
        
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);
        
        // Assert - should render all errors in a single panel
        var content = output.Content.GetContent();
        Assert.Contains("EditArrayTagHelper Configuration Error", content);
        Assert.Contains("ViewName", content);
        Assert.Contains("Items", content);
        Assert.Contains("id", content.ToLowerInvariant());
        
        // Verify it's rendered as a list
        Assert.Contains("<ul>", content);
        Assert.Contains("<li>", content);
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
    public async Task Should_RenderMaxItemsDataAttribute_When_MaxItemsIsSet()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.MaxItems = 5;

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.True(output.Attributes.ContainsName("data-max-items"));
        Assert.Equal("5", output.Attributes["data-max-items"].Value.ToString());
    }

    [Fact]
    public async Task Should_NotRenderMaxItemsDataAttribute_When_MaxItemsIsNull()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        tagHelper.MaxItems = null;

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.False(output.Attributes.ContainsName("data-max-items"));
    }

    [Fact]
    public async Task Should_RenderItemsContainerWithAriaLivePolite()
    {
        // Arrange
        var tagHelper = CreateTagHelper();
        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.Contains("id=\"edit-array-test-array-items\" aria-live=\"polite\"", content);
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
        Assert.Contains("class=\"edit-container ea-hidden\"", content);
        Assert.Contains("<input>", content); // Edit content
        
        // Check toggle buttons (use data attributes for event delegation, not inline onclick)
        Assert.Contains("data-action=\"edit\"", content);
        Assert.Contains("data-action=\"done\"", content);
        Assert.Contains("Edit", content); // Edit button text
        Assert.Contains("Done", content); // Done button text
    }

    [Fact]
    public async Task Should_RenderIndexedAriaLabels_ForItemActionButtons()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: new[] { "Item1", "Item2" }, viewName: "_Editor");
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
        Assert.Contains("aria-label=\"Edit item 1\"", content);
        Assert.Contains("aria-label=\"Delete item 1\"", content);
        Assert.Contains("aria-label=\"Done editing item 1\"", content);
        Assert.Contains("aria-label=\"Edit item 2\"", content);
        Assert.Contains("aria-label=\"Delete item 2\"", content);
        Assert.Contains("aria-label=\"Done editing item 2\"", content);

        // Ensure item IDs are no longer surfaced in button aria labels.
        Assert.DoesNotContain("aria-label=\"Edit item edit-array-test-array-item-0\"", content);
        Assert.DoesNotContain("aria-label=\"Delete item edit-array-test-array-item-0\"", content);
        Assert.DoesNotContain("aria-label=\"Done editing item edit-array-test-array-item-0\"", content);
    }

    [Fact]
    public async Task Should_RenderEditOnlyMode_When_DisplayViewNameIsNullAndDisplayModeIsFalse()
    {
        // Arrange
        var items = new[] { "Item1" };
        var tagHelper = CreateTagHelper(items: items, viewName: "_Editor");
        tagHelper.DisplayMode = false;
        tagHelper.DisplayViewName = null;

        SetupPartialAsync("_Editor", new HtmlString("<input>"));

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        Assert.DoesNotContain("class=\"display-container\"", content);
        Assert.Contains("class=\"edit-container\"", content);
        Assert.Contains("data-action=\"delete\"", content);
        Assert.DoesNotContain("data-action=\"edit\"", content);
        Assert.DoesNotContain("data-action=\"done\"", content);

        _htmlHelperMock.Verify(h => h.PartialAsync("_Display", It.IsAny<object>(), It.IsAny<ViewDataDictionary>()), Times.Never);
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
    public async Task Should_RenderEditOnlyTemplate_When_DisplayViewNameIsNull()
    {
        // Arrange
        var items = new List<TestItem> { new TestItem() };
        var tagHelper = CreateTagHelper(items: Array.Empty<object>(), viewName: "_Editor");
        tagHelper.Items = items;
        tagHelper.DisplayViewName = null;
        tagHelper.RenderTemplate = true;

        SetupPartialAsync("_Editor", new HtmlString("TemplateContent"));

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        var templateStart = content.IndexOf("<template", StringComparison.Ordinal);
        var templateEnd = content.IndexOf("</template>", StringComparison.Ordinal);
        var templateContent = content.Substring(templateStart, templateEnd - templateStart);

        Assert.DoesNotContain("display-container", templateContent);
        Assert.Contains("class=\"edit-container\"", templateContent);
        Assert.Contains("data-action=\"delete\"", templateContent);
        Assert.DoesNotContain("data-action=\"edit\"", templateContent);
        Assert.DoesNotContain("data-action=\"done\"", templateContent);

        _htmlHelperMock.Verify(h => h.PartialAsync("_Display", It.IsAny<object>(), It.IsAny<ViewDataDictionary>()), Times.Never);
    }

    [Fact]
    public async Task Should_RenderTemplateIsDeletedMarker_AsDirectChildOfItem()
    {
        // Arrange
        var items = new List<TestItem> { new TestItem() };
        var tagHelper = CreateTagHelper(items: Array.Empty<object>(), viewName: "_Editor");
        tagHelper.Items = items;
        tagHelper.RenderTemplate = true;

        SetupPartialAsync("_Editor", new HtmlString("TemplateContent"));
        SetupPartialAsync("_Display", new HtmlString("DisplayContent"));

        var context = CreateContext();
        var output = CreateOutput();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var content = output.Content.GetContent();
        var templateStart = content.IndexOf("<template", StringComparison.Ordinal);
        var templateEnd = content.IndexOf("</template>", StringComparison.Ordinal);
        var templateContent = content.Substring(templateStart, templateEnd - templateStart);

        var markerIndex = templateContent.IndexOf("data-is-deleted-marker", StringComparison.Ordinal);
        var displayContainerIndex = templateContent.IndexOf("class=\"display-container", StringComparison.Ordinal);
        var editContainerIndex = templateContent.IndexOf("class=\"edit-container", StringComparison.Ordinal);

        Assert.True(markerIndex >= 0, "Template should contain an IsDeleted marker input.");
        Assert.True(displayContainerIndex > markerIndex, "IsDeleted marker should be rendered before display container.");
        Assert.True(editContainerIndex > markerIndex, "IsDeleted marker should be rendered before edit container.");
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
        Assert.Contains("data-action=\"add\"", content);
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
        tagHelper.OnDone = "myDoneCallback";
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
        Assert.Contains("data-on-done=\"myDoneCallback\"", content);
        Assert.Contains("data-on-delete=\"myDeleteCallback\"", content);

        // Verify core button data-action attributes are present (event delegation, not inline onclick)
        Assert.Contains("data-action=\"delete\"", content);
        Assert.Contains("data-action=\"edit\"", content);
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_ArrayIdStartsWithDigit()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "1invalid");
        var context = CreateContext();
        var output = CreateOutput();

        // Act / Assert
        await Assert.ThrowsAsync<ArgumentException>(() => tagHelper.ProcessAsync(context, output));
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_OnUpdateCallbackIsInvalid()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: new[] { "Item1" }, viewName: "_Editor");
        tagHelper.OnUpdate = "alert('xss')";
        var context = CreateContext();
        var output = CreateOutput();

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => tagHelper.ProcessAsync(context, output));
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_OnDoneCallbackIsInvalid()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: new[] { "Item1" }, viewName: "_Editor");
        tagHelper.OnDone = "on.done";
        var context = CreateContext();
        var output = CreateOutput();

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => tagHelper.ProcessAsync(context, output));
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_OnDeleteCallbackIsInvalid()
    {
        // Arrange
        var tagHelper = CreateTagHelper(items: new[] { "Item1" }, viewName: "_Editor");
        tagHelper.OnDelete = "delete-item";
        var context = CreateContext();
        var output = CreateOutput();

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => tagHelper.ProcessAsync(context, output));
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
        // Template reorder buttons use data-item-id="closest" for event delegation
        Assert.Contains("data-item-id=\"closest\"", templateContent);
        Assert.Contains("data-action=\"move\"", templateContent);
        Assert.Contains("data-direction=\"-1\"", templateContent);
        Assert.Contains("data-direction=\"1\"", templateContent);
        Assert.DoesNotContain("data-reorder-direction", templateContent);
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
        Assert.Contains("data-action=\"move\"", content);
        Assert.Contains("data-direction=\"-1\"", content);
        Assert.Contains("data-direction=\"1\"", content);
        Assert.DoesNotContain("data-reorder-direction", content);
    }

    // ========================================================================
    // 8. Attribute Preservation Tests
    // ========================================================================

    [Fact]
    public async Task Should_PreserveUserProvidedClassAttribute_When_ClassIsSetOnElement()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "my-array");
        
        var context = CreateContext();
        // Create output with an existing class attribute (simulating user-provided class)
        var output = new TagHelperOutput(
            tagName: "edit-array",
            attributes: new TagHelperAttributeList { { "class", "my-custom-class another-class" } },
            getChildContentAsync: (useCached, encoder) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var classValue = output.Attributes["class"].Value.ToString();
        
        // Should contain both user-provided classes and container class
        Assert.Contains("my-custom-class", classValue);
        Assert.Contains("another-class", classValue);
        Assert.Contains("edit-array-container", classValue);
        
        // User-provided classes should come first
        Assert.StartsWith("my-custom-class another-class", classValue);
    }

    [Fact]
    public async Task Should_UseOnlyContainerClass_When_NoClassIsSetOnElement()
    {
        // Arrange
        var tagHelper = CreateTagHelper(id: "my-array");
        
        var context = CreateContext();
        var output = CreateOutput(); // No class attribute

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var classValue = output.Attributes["class"].Value.ToString();
        Assert.Equal("edit-array-container", classValue);
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
            ArrayId = id,
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

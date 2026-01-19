using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CK.Taghelpers.ViewComponents;
using Xunit;

namespace CK.TagHelpers.Tests.Views;

/// <summary>
/// Tests for the DynamicEditor default view.
///
/// Assumptions:
/// - The view is located at /Views/Shared/Components/DynamicEditor/Default.cshtml.
/// - The view renders inputs based on model metadata from IModelMetadataProvider.
/// </summary>
public class DynamicEditorDefaultViewTests : RazorViewTestBase
{
    private const string ViewPath = "/Views/Shared/Components/DynamicEditor/Default.cshtml";

    [Fact]
    public async Task should_render_dialog_and_actions_when_model_is_valid()
    {
        // Arrange
        var dialogId = "dialog-12345678";
        var viewModel = CreateViewModel(new HeaderModel(), "User", dialogId);

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains($"<dialog id=\"{dialogId}\" class=\"dynamic-editor-dialog\">", html);
        Assert.Contains($"<form method=\"dialog\" id=\"{dialogId}-form\">", html);
        Assert.Contains($"<h3>Edit {viewModel.EventName}</h3>", html);
        Assert.Contains($"<button type=\"button\" id=\"{dialogId}-cancel\">Cancel</button>", html);
        Assert.Contains($"<button type=\"button\" id=\"{dialogId}-confirm\">Confirm</button>", html);
    }

    [Fact]
    public async Task should_render_display_name_when_display_attribute_is_set()
    {
        // Arrange
        var viewModel = CreateViewModel(new DisplayNameModel { Name = "Alice" });

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains("for=\"Name\"", html);
        Assert.Contains("Full Name", html);
    }

    [Fact]
    public async Task should_render_checkbox_and_datetime_inputs_when_types_are_bool_and_datetime()
    {
        // Arrange
        var model = new BooleanDateModel
        {
            IsActive = true,
            StartDate = new DateTime(2024, 1, 2, 3, 4, 5)
        };
        var viewModel = CreateViewModel(model);

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains("type=\"checkbox\"", html);
        Assert.Contains("name=\"IsActive\"", html);
        Assert.Contains("checked", html);
        Assert.Contains("type=\"datetime-local\"", html);
        Assert.Contains("name=\"StartDate\"", html);
        Assert.Contains("value=\"2024-01-02T03:04:05\"", html);
    }

    [Fact]
    public async Task should_render_number_and_text_inputs_when_types_are_numeric_and_string()
    {
        // Arrange
        var model = new NumericTextModel
        {
            Age = 42,
            Price = 123m,
            Name = "Alice"
        };
        var viewModel = CreateViewModel(model);

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains("type=\"number\"", html);
        Assert.Contains("name=\"Age\"", html);
        Assert.Contains("value=\"42\"", html);
        Assert.Contains("name=\"Price\"", html);
        Assert.Contains("value=\"123\"", html);
        Assert.Contains("type=\"text\"", html);
        Assert.Contains("name=\"Name\"", html);
        Assert.Contains("value=\"Alice\"", html);
    }

    [Fact]
    public async Task should_render_enum_select_with_selected_value_when_enum_is_set()
    {
        // Arrange
        var model = new EnumModel { Status = TestStatus.Pending };
        var viewModel = CreateViewModel(model);

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains("<select name=\"Status\"", html);
        Assert.Contains("value=\"Pending\" selected", html);
    }

    [Fact]
    public async Task should_render_multi_select_with_selected_values_when_enum_collection_is_set()
    {
        // Arrange
        var model = new EnumCollectionModel
        {
            Roles = new List<TestStatus> { TestStatus.Active, TestStatus.Disabled }
        };
        var viewModel = CreateViewModel(model);

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains("name=\"Roles\"", html);
        Assert.Contains("multiple", html);
        Assert.Contains("data-collection=\"true\"", html);
        Assert.Contains("value=\"Active\" selected", html);
        Assert.Contains("value=\"Disabled\" selected", html);
    }

    [Fact]
    public async Task should_skip_complex_and_string_collection_properties_when_unsupported_types_exist()
    {
        // Arrange
        var viewModel = CreateViewModel(new UnsupportedTypesModel());

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.DoesNotContain("name=\"Address\"", html);
        Assert.DoesNotContain("name=\"Labels\"", html);
    }

    [Fact]
    public async Task should_initialize_dynamic_editor_script_when_rendered()
    {
        // Arrange
        var dialogId = "dialog-12345678";
        var eventName = "User";
        var viewModel = CreateViewModel(new HeaderModel(), eventName, dialogId);

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains("/_content/CK.Taghelpers/js/dynamicEditor.js", html);
        Assert.Contains($"DynamicEditor.init('{dialogId}', '{eventName}');", html);
    }

    private static DynamicEditorViewModel CreateViewModel(
        object dataModel,
        string eventName = "entity",
        string dialogId = "dialog-12345678")
    {
        return new DynamicEditorViewModel
        {
            DataModel = dataModel,
            EventName = eventName,
            DialogId = dialogId
        };
    }

    private sealed class HeaderModel
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class DisplayNameModel
    {
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;
    }

    private sealed class BooleanDateModel
    {
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
    }

    private sealed class NumericTextModel
    {
        public int Age { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class EnumModel
    {
        public TestStatus Status { get; set; }
    }

    private sealed class EnumCollectionModel
    {
        public List<TestStatus> Roles { get; set; } = new();
    }

    private sealed class UnsupportedTypesModel
    {
        public Address Address { get; set; } = new();
        public List<string> Labels { get; set; } = new();
    }

    private sealed class Address
    {
        public string Street { get; set; } = string.Empty;
    }

    private enum TestStatus
    {
        Active,
        Pending,
        Disabled
    }
}

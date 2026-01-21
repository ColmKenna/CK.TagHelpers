using System.ComponentModel.DataAnnotations;
using CK.Taghelpers.ViewComponents;
using Xunit;

namespace CK.TagHelpers.Tests.Views;

/// <summary>
/// Tests for the DynamicEditor default view.
///
/// Assumptions:
/// - The view is located at /Views/Shared/Components/DynamicEditor/Default.cshtml.
/// - The view renders inputs based on pre-built fields in the ViewModel.
/// </summary>
public class DynamicEditorDefaultViewTests : RazorViewTestBase
{
    private const string ViewPath = "/Views/Shared/Components/DynamicEditor/Default.cshtml";

    [Fact]
    public async Task should_render_dialog_and_actions_when_model_is_valid()
    {
        // Arrange
        var dialogId = "dialog-12345678";
        var viewModel = CreateViewModel(dialogId, "User", CreateTextField("Name", "Test", dialogId));

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains(
            $"<dialog id=\"{dialogId}\" class=\"dynamic-editor-dialog\" data-event-name=\"{viewModel.EventName}\">",
            html);
        Assert.Contains($"<form method=\"dialog\" id=\"{dialogId}-form\"", html);
        Assert.Contains($"<h3>Edit {viewModel.EventName}</h3>", html);
        Assert.Contains($"<button type=\"button\" id=\"{dialogId}-cancel\">Cancel</button>", html);
        Assert.Contains($"<button type=\"button\" id=\"{dialogId}-confirm\">Confirm</button>", html);
    }

    [Fact]
    public async Task should_render_event_name_as_data_attribute_when_event_name_is_provided()
    {
        // Arrange
        var eventName = "User";
        var viewModel = CreateViewModel("dialog-12345678", eventName, CreateTextField("Name", "Test", "dialog-12345678"));

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains($"data-event-name=\"{eventName}\"", html);
    }

    [Fact]
    public async Task should_render_display_name_when_display_name_is_set()
    {
        // Arrange
        var dialogId = "dialog-12345678";
        var field = new DynamicEditorField
        {
            PropertyName = "Name",
            DisplayName = "Full Name",
            InputId = $"{dialogId}_Name",
            InputType = FieldInputType.Text,
            FormattedValue = "Alice"
        };
        var viewModel = CreateViewModel(dialogId, "entity", field);

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains($"for=\"{dialogId}_Name\"", html);
        Assert.Contains("Full Name", html);
    }

    [Fact]
    public async Task should_render_checkbox_and_datetime_inputs_when_types_are_bool_and_datetime()
    {
        // Arrange
        var dialogId = "dialog-12345678";
        var checkboxField = new DynamicEditorField
        {
            PropertyName = "IsActive",
            DisplayName = "IsActive",
            InputId = $"{dialogId}_IsActive",
            InputType = FieldInputType.Checkbox,
            Value = true
        };
        var dateTimeField = new DynamicEditorField
        {
            PropertyName = "StartDate",
            DisplayName = "StartDate",
            InputId = $"{dialogId}_StartDate",
            InputType = FieldInputType.DateTime,
            FormattedValue = "2024-01-02T03:04:05"
        };
        var viewModel = CreateViewModel(dialogId, "entity", checkboxField, dateTimeField);

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
        var dialogId = "dialog-12345678";
        var ageField = new DynamicEditorField
        {
            PropertyName = "Age",
            DisplayName = "Age",
            InputId = $"{dialogId}_Age",
            InputType = FieldInputType.Number,
            FormattedValue = "42"
        };
        var priceField = new DynamicEditorField
        {
            PropertyName = "Price",
            DisplayName = "Price",
            InputId = $"{dialogId}_Price",
            InputType = FieldInputType.Number,
            FormattedValue = "123"
        };
        var nameField = new DynamicEditorField
        {
            PropertyName = "Name",
            DisplayName = "Name",
            InputId = $"{dialogId}_Name",
            InputType = FieldInputType.Text,
            FormattedValue = "Alice"
        };
        var viewModel = CreateViewModel(dialogId, "entity", ageField, priceField, nameField);

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
        var dialogId = "dialog-12345678";
        var field = new DynamicEditorField
        {
            PropertyName = "Status",
            DisplayName = "Status",
            InputId = $"{dialogId}_Status",
            InputType = FieldInputType.Select,
            Options = new List<SelectOption>
            {
                new() { Value = "Active", Text = "Active", IsSelected = false },
                new() { Value = "Pending", Text = "Pending", IsSelected = true },
                new() { Value = "Disabled", Text = "Disabled", IsSelected = false }
            }
        };
        var viewModel = CreateViewModel(dialogId, "entity", field);

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
        var dialogId = "dialog-12345678";
        var field = new DynamicEditorField
        {
            PropertyName = "Roles",
            DisplayName = "Roles",
            InputId = $"{dialogId}_Roles",
            InputType = FieldInputType.MultiSelect,
            Options = new List<SelectOption>
            {
                new() { Value = "Active", Text = "Active", IsSelected = true },
                new() { Value = "Pending", Text = "Pending", IsSelected = false },
                new() { Value = "Disabled", Text = "Disabled", IsSelected = true }
            }
        };
        var viewModel = CreateViewModel(dialogId, "entity", field);

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
    public async Task should_initialize_dynamic_editor_script_when_rendered()
    {
        // Arrange
        var dialogId = "dialog-12345678";
        var eventName = "User";
        var viewModel = CreateViewModel(dialogId, eventName, CreateTextField("Name", "Test", dialogId));

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains("/_content/CK.Taghelpers/js/dynamicEditor.js", html);
        Assert.Contains($"DynamicEditor.init('{dialogId}');", html);
        Assert.DoesNotContain($"DynamicEditor.init('{dialogId}',", html);
    }

    [Fact]
    public async Task should_render_required_indicator_when_field_is_required()
    {
        // Arrange
        var dialogId = "dialog-12345678";
        var field = new DynamicEditorField
        {
            PropertyName = "RequiredField",
            DisplayName = "Required Field",
            InputId = $"{dialogId}_RequiredField",
            InputType = FieldInputType.Text,
            IsRequired = true,
            ValidationAttributes = new Dictionary<string, string> { { "required", "required" } }
        };
        var viewModel = CreateViewModel(dialogId, "entity", field);

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains("<span class=\"required-indicator\">*</span>", html);
        Assert.Contains("required=\"required\"", html);
    }

    [Fact]
    public async Task should_render_email_input_when_field_type_is_email()
    {
        // Arrange
        var dialogId = "dialog-12345678";
        var field = new DynamicEditorField
        {
            PropertyName = "Email",
            DisplayName = "Email",
            InputId = $"{dialogId}_Email",
            InputType = FieldInputType.Email,
            FormattedValue = "test@example.com"
        };
        var viewModel = CreateViewModel(dialogId, "entity", field);

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains("type=\"email\"", html);
        Assert.Contains("value=\"test@example.com\"", html);
    }

    [Fact]
    public async Task should_render_textarea_when_field_type_is_textarea()
    {
        // Arrange
        var dialogId = "dialog-12345678";
        var field = new DynamicEditorField
        {
            PropertyName = "Description",
            DisplayName = "Description",
            InputId = $"{dialogId}_Description",
            InputType = FieldInputType.Textarea,
            FormattedValue = "Some long text"
        };
        var viewModel = CreateViewModel(dialogId, "entity", field);

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains("<textarea", html);
        Assert.Contains("name=\"Description\"", html);
        Assert.Contains("Some long text</textarea>", html);
    }

    [Fact]
    public async Task should_render_dialog_with_aria_labelledby_attribute()
    {
        // Arrange
        var dialogId = "dialog-12345678";
        var viewModel = CreateViewModel(dialogId, "User", CreateTextField("Name", "Test", dialogId));

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains($"aria-labelledby=\"{dialogId}-title\"", html);
        Assert.Contains($"id=\"{dialogId}-title\"", html);
    }

    [Fact]
    public async Task should_render_dialog_with_aria_modal_attribute()
    {
        // Arrange
        var dialogId = "dialog-12345678";
        var viewModel = CreateViewModel(dialogId, "User", CreateTextField("Name", "Test", dialogId));

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains("aria-modal=\"true\"", html);
    }

    [Fact]
    public async Task should_render_actions_group_with_role_and_aria_label()
    {
        // Arrange
        var dialogId = "dialog-12345678";
        var viewModel = CreateViewModel(dialogId, "User", CreateTextField("Name", "Test", dialogId));

        // Act
        var html = await RenderViewAsync(ViewPath, viewModel);

        // Assert
        Assert.Contains("role=\"group\"", html);
        Assert.Contains("aria-label=\"Dialog actions\"", html);
    }

    private static DynamicEditorViewModel CreateViewModel(
        string dialogId,
        string eventName,
        params DynamicEditorField[] fields)
    {
        return new DynamicEditorViewModel
        {
            EventName = eventName,
            DialogId = dialogId,
            Fields = fields.ToList()
        };
    }

    private static DynamicEditorField CreateTextField(string propertyName, string value, string dialogId)
    {
        return new DynamicEditorField
        {
            PropertyName = propertyName,
            DisplayName = propertyName,
            InputId = $"{dialogId}_{propertyName}",
            InputType = FieldInputType.Text,
            FormattedValue = value
        };
    }
}

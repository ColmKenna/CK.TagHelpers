using System.ComponentModel.DataAnnotations;
using CK.Taghelpers.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CK.TagHelpers.Tests.ViewComponents;

/// <summary>
/// Tests for <see cref="DynamicEditorViewComponent"/>
///
/// Dependencies:
/// - IModelMetadataProvider (provided via MvcCoreMvcBuilder services)
///
/// Test Coverage:
/// - Happy path: Builds fields from model, uses event name, generates dialog id
/// - Edge cases: Throws on null model, defaults empty/whitespace event names, throws on invalid characters
/// - Field building: Creates correct field types for different property types
/// - Async: InvokeAsync returns Task&lt;IViewComponentResult&gt;
///
/// Assumptions:
/// - The component validates model is not null
/// - Event names must contain only letters, digits, hyphens, and underscores
/// - Empty or whitespace event names default to "entity"
/// </summary>
public class DynamicEditorViewComponentTests : ViewComponentTestBase
{
    private readonly DynamicEditorViewComponent _sut;
    private readonly IModelMetadataProvider _metadataProvider;

    public DynamicEditorViewComponentTests()
    {
        _metadataProvider = CreateMetadataProvider();
        _sut = new DynamicEditorViewComponent(_metadataProvider);
        SetupViewComponentContext(_sut);
    }

    private static IModelMetadataProvider CreateMetadataProvider()
    {
        var services = new ServiceCollection();
        services.AddMvc();
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IModelMetadataProvider>();
    }

    #region Happy Path

    [Fact]
    public async Task should_build_fields_from_model_when_model_is_provided()
    {
        // Arrange
        var model = new TestModel { Name = "Test" };

        // Act
        var result = await _sut.InvokeAsync(model, "User");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        Assert.NotEmpty(viewModel.Fields);
        Assert.Contains(viewModel.Fields, f => f.PropertyName == "Name");
    }

    [Fact]
    public async Task should_set_field_value_from_model_property()
    {
        // Arrange
        var model = new TestModel { Name = "TestValue" };

        // Act
        var result = await _sut.InvokeAsync(model, "User");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        var nameField = viewModel.Fields.First(f => f.PropertyName == "Name");
        Assert.Equal("TestValue", nameField.FormattedValue);
    }

    [Fact]
    public async Task should_set_event_name_when_event_name_is_provided()
    {
        // Arrange
        var model = new TestModel { Name = "Test" };
        var eventName = "User";

        // Act
        var result = await _sut.InvokeAsync(model, eventName);

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        Assert.Equal(eventName, viewModel.EventName);
    }

    [Fact]
    public async Task should_use_default_event_name_when_event_name_is_not_provided()
    {
        // Arrange
        var model = new TestModel { Name = "Test" };

        // Act
        var result = await _sut.InvokeAsync(model);

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        Assert.Equal("entity", viewModel.EventName);
    }

    [Fact]
    public async Task should_generate_dialog_id_with_expected_format_when_invoked()
    {
        // Arrange
        var model = new TestModel { Name = "Test" };

        // Act
        var result = await _sut.InvokeAsync(model, "User");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        Assert.Matches("(?i)^dialog-[0-9a-f]{8}$", viewModel.DialogId);
    }

    [Fact]
    public async Task should_prefix_field_input_id_with_dialog_id()
    {
        // Arrange
        var model = new TestModel { Name = "Test" };

        // Act
        var result = await _sut.InvokeAsync(model, "User");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        var nameField = viewModel.Fields.First(f => f.PropertyName == "Name");
        Assert.StartsWith(viewModel.DialogId + "_", nameField.InputId);
    }

    #endregion

    #region Field Type Tests

    [Fact]
    public async Task should_create_checkbox_field_for_bool_property()
    {
        // Arrange
        var model = new TypedModel { IsActive = true };

        // Act
        var result = await _sut.InvokeAsync(model, "Test");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        var field = viewModel.Fields.First(f => f.PropertyName == "IsActive");
        Assert.Equal(FieldInputType.Checkbox, field.InputType);
    }

    [Fact]
    public async Task should_create_number_field_for_int_property()
    {
        // Arrange
        var model = new TypedModel { Count = 42 };

        // Act
        var result = await _sut.InvokeAsync(model, "Test");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        var field = viewModel.Fields.First(f => f.PropertyName == "Count");
        Assert.Equal(FieldInputType.Number, field.InputType);
        Assert.Equal("42", field.FormattedValue);
    }

    [Fact]
    public async Task should_create_datetime_field_for_datetime_property()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 10, 30, 0);
        var model = new TypedModel { CreatedAt = testDate };

        // Act
        var result = await _sut.InvokeAsync(model, "Test");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        var field = viewModel.Fields.First(f => f.PropertyName == "CreatedAt");
        Assert.Equal(FieldInputType.DateTime, field.InputType);
        Assert.Equal("2024-06-15T10:30:00", field.FormattedValue);
    }

    [Fact]
    public async Task should_create_select_field_for_enum_property()
    {
        // Arrange
        var model = new TypedModel { Status = TestStatus.Active };

        // Act
        var result = await _sut.InvokeAsync(model, "Test");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        var field = viewModel.Fields.First(f => f.PropertyName == "Status");
        Assert.Equal(FieldInputType.Select, field.InputType);
        Assert.Equal(3, field.Options.Count);
        Assert.True(field.Options.First(o => o.Value == "Active").IsSelected);
    }

    [Fact]
    public async Task should_create_email_field_when_email_attribute_present()
    {
        // Arrange
        var model = new ValidatedModel { Email = "test@example.com" };

        // Act
        var result = await _sut.InvokeAsync(model, "Test");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        var field = viewModel.Fields.First(f => f.PropertyName == "Email");
        Assert.Equal(FieldInputType.Email, field.InputType);
    }

    [Fact]
    public async Task should_mark_field_as_required_when_required_attribute_present()
    {
        // Arrange
        var model = new ValidatedModel { Email = "test@example.com" };

        // Act
        var result = await _sut.InvokeAsync(model, "Test");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        var field = viewModel.Fields.First(f => f.PropertyName == "RequiredField");
        Assert.True(field.IsRequired);
        Assert.Contains("required", field.ValidationAttributes.Keys);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task should_throw_argument_null_exception_when_model_is_null()
    {
        // Arrange
        object model = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.InvokeAsync(model, "User"));
    }

    [Fact]
    public async Task should_use_default_event_name_when_event_name_is_empty()
    {
        // Arrange
        var model = new TestModel { Name = "Test" };

        // Act
        var result = await _sut.InvokeAsync(model, string.Empty);

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        Assert.Equal("entity", viewModel.EventName);
    }

    [Fact]
    public async Task should_use_default_event_name_when_event_name_is_whitespace()
    {
        // Arrange
        var model = new TestModel { Name = "Test" };

        // Act
        var result = await _sut.InvokeAsync(model, "   ");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        Assert.Equal("entity", viewModel.EventName);
    }

    [Theory]
    [InlineData("User<script>")]
    [InlineData("User'")]
    [InlineData("User\"")]
    [InlineData("User Event")]
    [InlineData("User.Event")]
    public async Task should_throw_argument_exception_when_event_name_contains_invalid_characters(string eventName)
    {
        // Arrange
        var model = new TestModel { Name = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.InvokeAsync(model, eventName));
    }

    [Theory]
    [InlineData("User")]
    [InlineData("user-event")]
    [InlineData("user_event")]
    [InlineData("User123")]
    [InlineData("my-custom-event_name")]
    public async Task should_accept_valid_event_names(string eventName)
    {
        // Arrange
        var model = new TestModel { Name = "Test" };

        // Act
        var result = await _sut.InvokeAsync(model, eventName);

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        Assert.Equal(eventName, viewModel.EventName);
    }

    [Fact]
    public async Task should_skip_complex_type_properties()
    {
        // Arrange
        var model = new ModelWithComplexProperty
        {
            Name = "Test",
            Nested = new TestModel { Name = "Nested" }
        };

        // Act
        var result = await _sut.InvokeAsync(model, "Test");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        Assert.DoesNotContain(viewModel.Fields, f => f.PropertyName == "Nested");
        Assert.Contains(viewModel.Fields, f => f.PropertyName == "Name");
    }

    #endregion

    #region Test Models

    private sealed class TestModel
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TypedModel
    {
        public bool IsActive { get; set; }
        public int Count { get; set; }
        public DateTime CreatedAt { get; set; }
        public TestStatus Status { get; set; }
    }

    private enum TestStatus
    {
        Pending,
        Active,
        Completed
    }

    private sealed class ValidatedModel
    {
        [Required]
        public string RequiredField { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    private sealed class ModelWithComplexProperty
    {
        public string Name { get; set; } = string.Empty;
        public TestModel Nested { get; set; } = null!;
    }

    #endregion
}

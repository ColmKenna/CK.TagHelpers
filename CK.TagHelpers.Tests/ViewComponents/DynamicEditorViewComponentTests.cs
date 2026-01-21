using CK.Taghelpers.ViewComponents;
using Xunit;

namespace CK.TagHelpers.Tests.ViewComponents;

/// <summary>
/// Tests for <see cref="DynamicEditorViewComponent"/>
///
/// Dependencies:
/// - None
///
/// Test Coverage:
/// - Happy path: Wraps provided model and event name, uses default event name, generates dialog id
/// - Edge cases: Throws on null model, defaults empty/whitespace event names, throws on invalid characters
/// - Async: InvokeAsync returns Task<IViewComponentResult>
///
/// Assumptions:
/// - The component validates model is not null
/// - Event names must contain only letters, digits, hyphens, and underscores
/// - Empty or whitespace event names default to "entity"
/// </summary>
public class DynamicEditorViewComponentTests : ViewComponentTestBase
{
    private readonly DynamicEditorViewComponent _sut;

    public DynamicEditorViewComponentTests()
    {
        _sut = new DynamicEditorViewComponent();
        SetupViewComponentContext(_sut);
    }

    #region Happy Path

    [Fact]
    public async Task should_set_data_model_when_model_is_provided()
    {
        // Arrange
        var model = new TestModel { Name = "Test" };

        // Act
        var result = await _sut.InvokeAsync(model, "User");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        Assert.Same(model, viewModel.DataModel);
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

    #endregion

    private sealed class TestModel
    {
        public string Name { get; set; } = string.Empty;
    }
}

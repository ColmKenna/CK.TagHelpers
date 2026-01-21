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
/// - Edge cases: Accepts null model, preserves empty event name
/// - Async: InvokeAsync returns Task<IViewComponentResult>
///
/// Assumptions:
/// - The component does not perform parameter validation; null model and empty event names are allowed.
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
    public async Task should_allow_null_model_when_model_is_null()
    {
        // Arrange
        object model = null!;

        // Act
        var result = await _sut.InvokeAsync(model, "User");

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        Assert.Null(viewModel.DataModel);
    }

    [Fact]
    public async Task should_preserve_empty_event_name_when_event_name_is_empty()
    {
        // Arrange
        var model = new TestModel { Name = "Test" };

        // Act
        var result = await _sut.InvokeAsync(model, string.Empty);

        // Assert
        var viewModel = GetViewModel<DynamicEditorViewModel>(result);
        Assert.Equal(string.Empty, viewModel.EventName);
    }

    #endregion

    private sealed class TestModel
    {
        public string Name { get; set; } = string.Empty;
    }
}

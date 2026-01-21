using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace CK.Taghelpers.ViewComponents;

/// <summary>
/// A ViewComponent that renders a dynamic editor dialog for any model.
/// It automatically generates form fields based on the model's properties
/// and dispatches custom events for confirm/cancel actions.
/// </summary>
public partial class DynamicEditorViewComponent : ViewComponent
{
    private const string DefaultEventName = "entity";

    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    private static partial Regex SafeEventNamePattern();

    /// <summary>
    /// Invokes the ViewComponent asynchronously to render a dynamic editor dialog.
    /// </summary>
    /// <param name="model">The model object to edit. Properties will be reflected to create form fields.</param>
    /// <param name="eventName">The prefix for custom events dispatched by the dialog (e.g., "User" creates "User-update" and "User-cancel" events). Must contain only letters, digits, hyphens, and underscores.</param>
    /// <returns>A task representing the asynchronous operation, containing the rendered view result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when model is null.</exception>
    /// <exception cref="ArgumentException">Thrown when eventName contains invalid characters.</exception>
    public Task<IViewComponentResult> InvokeAsync(object model, string eventName = DefaultEventName)
    {
        ArgumentNullException.ThrowIfNull(model);

        // Use default if null or whitespace
        var validatedEventName = string.IsNullOrWhiteSpace(eventName) ? DefaultEventName : eventName;

        // Validate eventName contains only safe characters
        if (!SafeEventNamePattern().IsMatch(validatedEventName))
        {
            throw new ArgumentException(
                "Event name must contain only letters, digits, hyphens, and underscores.",
                nameof(eventName));
        }

        var wrapper = new DynamicEditorViewModel
        {
            DataModel = model,
            EventName = validatedEventName,
            // Create a unique ID for the dialog to avoid conflicts
            DialogId = $"dialog-{Guid.NewGuid().ToString("N")[..8]}"
        };

        return Task.FromResult<IViewComponentResult>(View(wrapper));
    }
}

/// <summary>
/// ViewModel for the DynamicEditor ViewComponent that wraps the data model
/// along with configuration for the editor dialog.
/// </summary>
public class DynamicEditorViewModel
{
    /// <summary>
    /// The data model object whose properties will be edited.
    /// </summary>
    public object DataModel { get; set; } = null!;

    /// <summary>
    /// The event name prefix used for dispatching custom events.
    /// Events dispatched will be "{EventName}-update" and "{EventName}-cancel".
    /// </summary>
    public string EventName { get; set; } = "entity";

    /// <summary>
    /// A unique identifier for the dialog element to avoid DOM conflicts
    /// when multiple editors are present on the same page.
    /// </summary>
    public string DialogId { get; set; } = string.Empty;
}

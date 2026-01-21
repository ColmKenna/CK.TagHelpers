using Microsoft.AspNetCore.Mvc;

namespace CK.Taghelpers.ViewComponents;

/// <summary>
/// A ViewComponent that renders a dynamic editor dialog for any model.
/// It automatically generates form fields based on the model's properties
/// and dispatches custom events for confirm/cancel actions.
/// </summary>
public class DynamicEditorViewComponent : ViewComponent
{
    /// <summary>
    /// Invokes the ViewComponent asynchronously to render a dynamic editor dialog.
    /// </summary>
    /// <param name="model">The model object to edit. Properties will be reflected to create form fields.</param>
    /// <param name="eventName">The prefix for custom events dispatched by the dialog (e.g., "User" creates "User-update" and "User-cancel" events).</param>
    /// <returns>A task representing the asynchronous operation, containing the rendered view result.</returns>
    public Task<IViewComponentResult> InvokeAsync(object model, string eventName = "entity")
    {
        ArgumentNullException.ThrowIfNull(model);

        var wrapper = new DynamicEditorViewModel
        {
            DataModel = model,
            EventName = eventName,
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

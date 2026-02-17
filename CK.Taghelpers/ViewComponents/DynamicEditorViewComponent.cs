using System.Collections;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CK.Taghelpers.ViewComponents;

/// <summary>
/// The type of input to render for a field.
/// </summary>
public enum FieldInputType
{
    Text,
    Number,
    Checkbox,
    DateTime,
    Date,
    Time,
    Email,
    Password,
    Tel,
    Url,
    Textarea,
    Select,
    MultiSelect
}

/// <summary>
/// Represents an option in a select/dropdown field.
/// </summary>
public class SelectOption
{
    /// <summary>
    /// The value attribute for the option.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// The display text for the option.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Whether this option is currently selected.
    /// </summary>
    public bool IsSelected { get; set; }
}

/// <summary>
/// Represents a single editable field in the dynamic editor.
/// </summary>
public class DynamicEditorField
{
    /// <summary>
    /// The property name used for form submission.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// The display label for the field.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The unique ID for the input element.
    /// </summary>
    public string InputId { get; set; } = string.Empty;

    /// <summary>
    /// The type of input to render.
    /// </summary>
    public FieldInputType InputType { get; set; } = FieldInputType.Text;

    /// <summary>
    /// The current value of the field.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// The formatted value for display (e.g., date formatted as ISO string).
    /// </summary>
    public string? FormattedValue { get; set; }

    /// <summary>
    /// Whether the field is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// HTML validation attributes to render on the input.
    /// </summary>
    public Dictionary<string, string> ValidationAttributes { get; set; } = new();

    /// <summary>
    /// Options for select/dropdown fields.
    /// </summary>
    public List<SelectOption> Options { get; set; } = new();
}

/// <summary>
/// ViewModel for the DynamicEditor ViewComponent that wraps the data model
/// along with configuration for the editor dialog.
/// </summary>
public class DynamicEditorViewModel
{
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

    /// <summary>
    /// The list of fields to render in the editor.
    /// </summary>
    public List<DynamicEditorField> Fields { get; set; } = new();
}

/// <summary>
/// A ViewComponent that renders a dynamic editor dialog for any model.
/// It automatically generates form fields based on the model's properties
/// and dispatches custom events for confirm/cancel actions.
/// </summary>
public class DynamicEditorViewComponent : ViewComponent
{
    private const string DefaultEventName = "entity";
    private readonly IModelMetadataProvider _metadataProvider;

    public DynamicEditorViewComponent(IModelMetadataProvider metadataProvider)
    {
        _metadataProvider = metadataProvider;
    }

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
        if (!ValidationRegex.SafeEventName().IsMatch(validatedEventName))
        {
            throw new ArgumentException(
                "Event name must contain only letters, digits, hyphens, and underscores.",
                nameof(eventName));
        }

        var dialogId = GenerateDialogId();
        var fields = BuildFields(model, dialogId);

        var viewModel = new DynamicEditorViewModel
        {
            EventName = validatedEventName,
            DialogId = dialogId,
            Fields = fields
        };

        return Task.FromResult<IViewComponentResult>(View(viewModel));
    }

    private List<DynamicEditorField> BuildFields(object model, string dialogId)
    {
        var fields = new List<DynamicEditorField>();
        var modelMetadata = _metadataProvider.GetMetadataForType(model.GetType());

        foreach (var propertyMetadata in modelMetadata.Properties)
        {
            var field = BuildField(propertyMetadata, model, dialogId);
            if (field != null)
            {
                fields.Add(field);
            }
        }

        return fields;
    }

    private DynamicEditorField? BuildField(ModelMetadata propertyMetadata, object model, string dialogId)
    {
        // Check if this is a collection type
        var isCollection = propertyMetadata.IsCollectionType &&
                          !propertyMetadata.ModelType.IsAssignableFrom(typeof(string));
        Type? elementType = null;
        var isEnumCollection = false;

        if (isCollection && propertyMetadata.ElementMetadata != null)
        {
            elementType = propertyMetadata.ElementMetadata.ModelType;
            isEnumCollection = elementType?.IsEnum ?? false;
        }

        // Skip complex types except DateTime and supported collections
        if (propertyMetadata.IsComplexType &&
            propertyMetadata.ModelType != typeof(DateTime) &&
            !isEnumCollection)
        {
            return null;
        }

        var propertyName = propertyMetadata.PropertyName ?? string.Empty;
        var value = propertyMetadata.PropertyGetter?.Invoke(model);
        var validationAttrs = GetValidationAttributes(propertyMetadata);

        var field = new DynamicEditorField
        {
            PropertyName = propertyName,
            DisplayName = propertyMetadata.DisplayName ?? propertyName,
            InputId = $"{dialogId}_{propertyName}",
            Value = value,
            IsRequired = validationAttrs.ContainsKey("required"),
            ValidationAttributes = validationAttrs
        };

        // Determine input type and handle special cases
        DetermineInputType(field, propertyMetadata, value, validationAttrs, isEnumCollection, elementType);

        return field;
    }

    private void DetermineInputType(
        DynamicEditorField field,
        ModelMetadata metadata,
        object? value,
        Dictionary<string, string> validationAttrs,
        bool isEnumCollection,
        Type? elementType)
    {
        var modelType = metadata.ModelType;

        if (modelType == typeof(bool))
        {
            field.InputType = FieldInputType.Checkbox;
            field.FormattedValue = (value as bool? == true).ToString().ToLowerInvariant();
        }
        else if (modelType == typeof(DateTime) || modelType == typeof(DateTime?))
        {
            // Check for specific date/time types from validation attributes
            if (validationAttrs.TryGetValue("type", out var dateType))
            {
                field.InputType = dateType switch
                {
                    "date" => FieldInputType.Date,
                    "time" => FieldInputType.Time,
                    _ => FieldInputType.DateTime
                };
                validationAttrs.Remove("type");
            }
            else
            {
                field.InputType = FieldInputType.DateTime;
            }
            field.FormattedValue = (value as DateTime?)?.ToString("s");
        }
        else if (isEnumCollection && elementType != null)
        {
            field.InputType = FieldInputType.MultiSelect;
            var selectedValues = new HashSet<string>();

            if (value is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        selectedValues.Add(item.ToString()!);
                    }
                }
            }

            field.Options = Enum.GetNames(elementType)
                .Select(name => new SelectOption
                {
                    Value = name,
                    Text = name,
                    IsSelected = selectedValues.Contains(name)
                })
                .ToList();
        }
        else if (metadata.IsEnum)
        {
            field.InputType = FieldInputType.Select;
            var currentValue = value?.ToString();

            field.Options = Enum.GetNames(modelType)
                .Select(name => new SelectOption
                {
                    Value = name,
                    Text = name,
                    IsSelected = name == currentValue
                })
                .ToList();
        }
        else if (IsNumericType(modelType))
        {
            field.InputType = FieldInputType.Number;
            field.FormattedValue = value?.ToString();
        }
        else if (validationAttrs.ContainsKey("data-multiline"))
        {
            field.InputType = FieldInputType.Textarea;
            field.FormattedValue = value?.ToString();
            validationAttrs.Remove("data-multiline");
        }
        else
        {
            // Check for type from validation attributes (email, password, tel, url)
            if (validationAttrs.TryGetValue("type", out var inputType))
            {
                field.InputType = inputType switch
                {
                    "email" => FieldInputType.Email,
                    "password" => FieldInputType.Password,
                    "tel" => FieldInputType.Tel,
                    "url" => FieldInputType.Url,
                    _ => FieldInputType.Text
                };
                validationAttrs.Remove("type");
            }
            else
            {
                field.InputType = FieldInputType.Text;
            }
            field.FormattedValue = value?.ToString();
        }
    }

    /// <summary>
    /// Generates a unique dialog ID using GUID bytes directly to avoid intermediate string allocation.
    /// </summary>
    private static string GenerateDialogId()
    {
        Span<byte> bytes = stackalloc byte[16];
        Guid.NewGuid().TryWriteBytes(bytes);
        return $"dialog-{bytes[0]:x2}{bytes[1]:x2}{bytes[2]:x2}{bytes[3]:x2}";
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(int?) ||
               type == typeof(long) || type == typeof(long?) ||
               type == typeof(decimal) || type == typeof(decimal?) ||
               type == typeof(double) || type == typeof(double?) ||
               type == typeof(float) || type == typeof(float?);
    }

    private static Dictionary<string, string> GetValidationAttributes(ModelMetadata metadata)
    {
        var attrs = new Dictionary<string, string>();
        var validatorMetadata = metadata.ValidatorMetadata;

        foreach (var validator in validatorMetadata)
        {
            switch (validator)
            {
                case RequiredAttribute required:
                    attrs["required"] = "required";
                    if (!string.IsNullOrEmpty(required.ErrorMessage))
                    {
                        attrs["data-val-required"] = required.ErrorMessage;
                    }
                    break;

                case MinLengthAttribute minLength:
                    attrs["minlength"] = minLength.Length.ToString();
                    if (!string.IsNullOrEmpty(minLength.ErrorMessage))
                    {
                        attrs["data-val-minlength"] = minLength.ErrorMessage;
                    }
                    break;

                case MaxLengthAttribute maxLength:
                    attrs["maxlength"] = maxLength.Length.ToString();
                    if (!string.IsNullOrEmpty(maxLength.ErrorMessage))
                    {
                        attrs["data-val-maxlength"] = maxLength.ErrorMessage;
                    }
                    break;

                case StringLengthAttribute stringLength:
                    if (stringLength.MinimumLength > 0)
                    {
                        attrs["minlength"] = stringLength.MinimumLength.ToString();
                    }
                    attrs["maxlength"] = stringLength.MaximumLength.ToString();
                    break;

                case RangeAttribute range:
                    if (range.Minimum != null)
                    {
                        attrs["min"] = range.Minimum.ToString()!;
                    }
                    if (range.Maximum != null)
                    {
                        attrs["max"] = range.Maximum.ToString()!;
                    }
                    if (!string.IsNullOrEmpty(range.ErrorMessage))
                    {
                        attrs["data-val-range"] = range.ErrorMessage;
                    }
                    break;

                case RegularExpressionAttribute regex:
                    attrs["pattern"] = regex.Pattern;
                    if (!string.IsNullOrEmpty(regex.ErrorMessage))
                    {
                        attrs["data-val-regex"] = regex.ErrorMessage;
                    }
                    break;

                case EmailAddressAttribute:
                    attrs["type"] = "email";
                    break;

                case PhoneAttribute:
                    attrs["type"] = "tel";
                    break;

                case UrlAttribute:
                    attrs["type"] = "url";
                    break;

                case DataTypeAttribute dataType:
                    switch (dataType.DataType)
                    {
                        case DataType.Password:
                            attrs["type"] = "password";
                            break;
                        case DataType.EmailAddress:
                            attrs["type"] = "email";
                            break;
                        case DataType.PhoneNumber:
                            attrs["type"] = "tel";
                            break;
                        case DataType.Url:
                            attrs["type"] = "url";
                            break;
                        case DataType.Date:
                            attrs["type"] = "date";
                            break;
                        case DataType.Time:
                            attrs["type"] = "time";
                            break;
                        case DataType.MultilineText:
                            attrs["data-multiline"] = "true";
                            break;
                    }
                    break;
            }
        }

        return attrs;
    }
}

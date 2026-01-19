# DynamicEditor ViewComponent - Complete Documentation

> **📖 Note**: This is a Markdown file. For the best reading experience, open it in your IDE's Markdown viewer.
> If you're browsing the web application, visit `/Home/DynamicEditor` to see live examples.

This document provides comprehensive documentation for the `DynamicEditorViewComponent` from the `CK.Taghelpers` library.

## What is the DynamicEditor ViewComponent?

The DynamicEditor ViewComponent renders a **dialog-based form editor** that automatically generates input fields based on any model you pass to it. It uses reflection to discover model properties and creates appropriate HTML inputs for each type, dispatching custom JavaScript events when the user confirms or cancels.

### Key Features

- **Automatic Field Generation**: Reflects model properties to create form fields
- **Type-Aware Inputs**: Generates appropriate input types (text, number, checkbox, datetime, select)
- **Custom Event Dispatching**: Fires `{eventName}-update` and `{eventName}-cancel` events
- **Multiple Instance Support**: Each instance gets a unique dialog ID to prevent conflicts
- **Native Dialog Element**: Uses the HTML `<dialog>` element for modal behavior
- **No External Dependencies**: Pure JavaScript, no libraries required

## Quick Start

### 1. Register the ViewComponent

In `_ViewImports.cshtml`:

```cshtml
@addTagHelper *, CK.Taghelpers
```

### 2. Use the ViewComponent

```cshtml
@{
    var userModel = new { FirstName = "John", LastName = "Doe", IsActive = true };
}

<vc:dynamic-editor model="@userModel" event-name="User"></vc:dynamic-editor>

<button onclick="document.querySelector('dialog').showModal()">Edit User</button>
```

### 3. Handle Events

```html
<script>
    document.addEventListener('User-update', function (e) {
        console.log('Updated:', e.detail);
        // e.detail contains: { FirstName: "John", LastName: "Doe", IsActive: "on" }
    });

    document.addEventListener('User-cancel', function (e) {
        console.log('Cancelled');
    });
</script>
```

## Parameters Reference

### ViewComponent Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `model` | object | **required** | The model object whose properties will be edited |
| `event-name` | string | `"entity"` | Prefix for custom events dispatched by the dialog |

### Events Dispatched

| Event Name | Triggered When | Event Detail |
|------------|----------------|--------------|
| `{eventName}-update` | User clicks Confirm | Object with form field values |
| `{eventName}-cancel` | User clicks Cancel | Empty (no detail) |

## Supported Property Types

The ViewComponent automatically generates appropriate inputs for:

| Type | Input Type | Notes |
|------|------------|-------|
| `string` | `<input type="text">` | Default for unknown types |
| `int`, `long`, `decimal`, `double`, `float` | `<input type="number">` | Including nullable variants |
| `bool` | `<input type="checkbox">` | Checked state preserved |
| `DateTime` | `<input type="datetime-local">` | Formatted as ISO 8601 |
| `enum` | `<select>` | Options from enum values |

**Note**: Complex types and collections are automatically skipped (except `DateTime`).

## Generated HTML Structure

For a simple model:

```cshtml
@{
    var model = new { Name = "John", IsActive = true };
}
<vc:dynamic-editor model="@model" event-name="User"></vc:dynamic-editor>
```

The ViewComponent generates:

```html
<dialog id="dialog-a1b2c3d4" class="dynamic-editor-dialog">
    <form method="dialog" id="dialog-a1b2c3d4-form">
        <h3>Edit User</h3>
        
        <div class="editor-fields">
            <div class="form-group">
                <label for="Name">Name</label>
                <input type="text" name="Name" id="Name" value="John" />
            </div>
            <div class="form-group">
                <label for="IsActive">IsActive</label>
                <input type="checkbox" name="IsActive" id="IsActive" checked />
            </div>
        </div>
        
        <div class="actions">
            <button type="button" id="dialog-a1b2c3d4-cancel">Cancel</button>
            <button type="button" id="dialog-a1b2c3d4-confirm">Confirm</button>
        </div>
    </form>
</dialog>
```

## CSS Classes Reference

| Class | Element | Description |
|-------|---------|-------------|
| `.dynamic-editor-dialog` | `<dialog>` | Main dialog container |
| `.editor-fields` | `<div>` | Container for all form fields |
| `.form-group` | `<div>` | Wrapper for each label + input pair |
| `.actions` | `<div>` | Container for Cancel and Confirm buttons |

## Styling

### Basic Styling Example

```css
.dynamic-editor-dialog {
    border: none;
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    padding: 0;
    max-width: 500px;
    width: 90%;
}

.dynamic-editor-dialog::backdrop {
    background: rgba(0, 0, 0, 0.5);
}

.dynamic-editor-dialog form {
    padding: 1.5rem;
}

.dynamic-editor-dialog h3 {
    margin: 0 0 1.5rem 0;
    padding-bottom: 1rem;
    border-bottom: 1px solid #dee2e6;
}

.dynamic-editor-dialog .form-group {
    margin-bottom: 1rem;
}

.dynamic-editor-dialog label {
    display: block;
    margin-bottom: 0.25rem;
    font-weight: 500;
}

.dynamic-editor-dialog input,
.dynamic-editor-dialog select {
    width: 100%;
    padding: 0.5rem;
    border: 1px solid #ced4da;
    border-radius: 4px;
}

.dynamic-editor-dialog input[type="checkbox"] {
    width: auto;
}

.dynamic-editor-dialog .actions {
    display: flex;
    justify-content: flex-end;
    gap: 0.5rem;
    margin-top: 1.5rem;
    padding-top: 1rem;
    border-top: 1px solid #dee2e6;
}
```

## Common Patterns

### Multiple Editors on One Page

```cshtml
@{
    var user = new { Name = "John" };
    var product = new { Title = "Widget", Price = 9.99 };
}

<vc:dynamic-editor model="@user" event-name="User"></vc:dynamic-editor>
<vc:dynamic-editor model="@product" event-name="Product"></vc:dynamic-editor>

<script>
    // Each dialog has a unique ID, but you can find them by class
    const dialogs = document.querySelectorAll('.dynamic-editor-dialog');
    
    // Or listen for events by name
    document.addEventListener('User-update', e => console.log('User:', e.detail));
    document.addEventListener('Product-update', e => console.log('Product:', e.detail));
</script>
```

### Opening a Specific Dialog

Since each dialog gets a unique ID, you need to either:

1. **Query by class and index**:
```javascript
const dialogs = document.querySelectorAll('.dynamic-editor-dialog');
dialogs[0].showModal(); // First dialog
```

2. **Assign a custom ID after render**:
```javascript
document.addEventListener('DOMContentLoaded', () => {
    const dialogs = document.querySelectorAll('.dynamic-editor-dialog');
    dialogs[0].id = 'user-dialog';
    dialogs[1].id = 'product-dialog';
});

// Then open by ID
document.getElementById('user-dialog').showModal();
```

### Handling Form Data

The `update` event detail contains an object with field names as keys:

```javascript
document.addEventListener('User-update', function (e) {
    const data = e.detail;
    
    // Send to server
    fetch('/api/users', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    });
});
```

**Note**: Checkbox values are `"on"` when checked, and absent when unchecked (standard HTML behavior).

### Using with Strongly-Typed Models

```cshtml
@model MyViewModel

<vc:dynamic-editor model="@Model.User" event-name="EditUser"></vc:dynamic-editor>
```

## Browser Support

The DynamicEditor uses the native HTML `<dialog>` element, which is supported in:

- Chrome 37+
- Firefox 98+
- Safari 15.4+
- Edge 79+

For older browsers, consider using a polyfill like [dialog-polyfill](https://github.com/GoogleChrome/dialog-polyfill).

## Limitations

1. **Complex Types**: Nested objects and collections are skipped
2. **Validation**: No built-in validation - add your own in the update handler
3. **Display Names**: Uses `DisplayName` attribute if present, otherwise property name
4. **Read-Only**: Currently no support for excluding specific properties

## Examples

Visit `/Home/DynamicEditor` in the sample application to see:

1. **User Editor**: Basic user profile editing
2. **Product Editor**: Product with price (number input)
3. **Settings Editor**: Application settings

## Related Documentation

- [DYNAMICEDITOR-QUICKSTART.md](DYNAMICEDITOR-QUICKSTART.md) - Get started in 5 minutes
- [DYNAMICEDITOR-API-REFERENCE.md](DYNAMICEDITOR-API-REFERENCE.md) - Quick parameter lookup

## License

This component is part of the CK.Taghelpers library and is provided under the MIT license.

# DynamicEditor ViewComponent - API Reference

Quick reference for the DynamicEditor ViewComponent parameters, events, and CSS classes.

## ViewComponent Invocation

```cshtml
<vc:dynamic-editor model="@myModel" event-name="MyEvent"></vc:dynamic-editor>
```

## Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `model` | `object` | ✅ Yes | - | The data model to edit |
| `event-name` | `string` | No | `"entity"` | Prefix for dispatched events |

## Events

### {eventName}-update

Fired when user clicks Confirm.

```javascript
document.addEventListener('User-update', function (e) {
    console.log(e.detail); // { fieldName: "value", ... }
});
```

**Event Detail**: Object containing form field names and values.

### {eventName}-cancel

Fired when user clicks Cancel.

```javascript
document.addEventListener('User-cancel', function () {
    console.log('Cancelled');
});
```

**Event Detail**: None

## Type Mappings

| C# Type | HTML Input | Notes |
|---------|------------|-------|
| `string` | `<input type="text">` | Default |
| `int`, `int?` | `<input type="number">` | |
| `long`, `long?` | `<input type="number">` | |
| `decimal`, `decimal?` | `<input type="number">` | |
| `double`, `double?` | `<input type="number">` | |
| `float`, `float?` | `<input type="number">` | |
| `bool` | `<input type="checkbox">` | |
| `DateTime`, `DateTime?` | `<input type="datetime-local">` | ISO format |
| `enum` | `<select>` | Options from enum names |
| Complex types | *Skipped* | Except DateTime |

## CSS Classes

| Class | Applied To | Description |
|-------|------------|-------------|
| `.dynamic-editor-dialog` | `<dialog>` | Main container |
| `.editor-fields` | `<div>` | Field container |
| `.form-group` | `<div>` | Label + input wrapper |
| `.actions` | `<div>` | Button container |

## HTML Structure

```html
<dialog id="dialog-{uniqueId}" class="dynamic-editor-dialog">
    <form method="dialog" id="dialog-{uniqueId}-form">
        <h3>Edit {eventName}</h3>
        <div class="editor-fields">
            <div class="form-group">
                <label for="{propertyName}">{displayName}</label>
                <input type="{type}" name="{propertyName}" id="{propertyName}" value="{value}" />
            </div>
            <!-- ... more fields ... -->
        </div>
        <div class="actions">
            <button type="button" id="dialog-{uniqueId}-cancel">Cancel</button>
            <button type="button" id="dialog-{uniqueId}-confirm">Confirm</button>
        </div>
    </form>
</dialog>
```

## Dialog Methods

Native `<dialog>` element methods:

```javascript
const dialog = document.querySelector('.dynamic-editor-dialog');

// Open as modal (with backdrop)
dialog.showModal();

// Open non-modal
dialog.show();

// Close
dialog.close();

// Check if open
console.log(dialog.open); // true/false
```

## ViewModel Classes

```csharp
// ViewComponent class
namespace CK.Taghelpers.ViewComponents
{
    public class DynamicEditorViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(object model, string eventName = "entity");
    }

    public class DynamicEditorViewModel
    {
        public object DataModel { get; set; }
        public string EventName { get; set; }
        public string DialogId { get; set; }
    }
}
```

## Example CSS

```css
.dynamic-editor-dialog {
    border: none;
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    max-width: 500px;
}

.dynamic-editor-dialog::backdrop {
    background: rgba(0, 0, 0, 0.5);
}
```

## See Also

- [DYNAMICEDITOR-QUICKSTART.md](DYNAMICEDITOR-QUICKSTART.md) - Quick start guide
- [DYNAMICEDITOR-README.md](DYNAMICEDITOR-README.md) - Full documentation

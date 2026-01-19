# DynamicEditor ViewComponent - Quick Start

> Get the DynamicEditor running in 5 minutes!

## Prerequisites

- ASP.NET Core project
- Reference to `CK.Taghelpers` library

## Step 1: Register the TagHelpers

In `_ViewImports.cshtml`:

```cshtml
@addTagHelper *, CK.Taghelpers
```

## Step 2: Create a Simple Editor

In any Razor view:

```cshtml
@{
    var myModel = new { 
        FirstName = "John", 
        LastName = "Doe", 
        Age = 30,
        IsActive = true 
    };
}

<!-- The ViewComponent -->
<vc:dynamic-editor model="@myModel" event-name="MyForm"></vc:dynamic-editor>

<!-- Button to open the dialog -->
<button onclick="document.querySelector('.dynamic-editor-dialog').showModal()">
    Edit
</button>

<!-- Handle the events -->
<script>
    document.addEventListener('MyForm-update', function (e) {
        console.log('Data saved:', e.detail);
        alert('Saved: ' + JSON.stringify(e.detail));
    });

    document.addEventListener('MyForm-cancel', function () {
        console.log('Cancelled');
    });
</script>
```

## Step 3: Add Basic Styling (Optional)

```html
<style>
    .dynamic-editor-dialog {
        border: none;
        border-radius: 8px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
        padding: 1.5rem;
        max-width: 400px;
    }

    .dynamic-editor-dialog::backdrop {
        background: rgba(0, 0, 0, 0.5);
    }

    .dynamic-editor-dialog .form-group {
        margin-bottom: 1rem;
    }

    .dynamic-editor-dialog label {
        display: block;
        font-weight: bold;
        margin-bottom: 0.25rem;
    }

    .dynamic-editor-dialog input,
    .dynamic-editor-dialog select {
        width: 100%;
        padding: 0.5rem;
        border: 1px solid #ccc;
        border-radius: 4px;
    }

    .dynamic-editor-dialog input[type="checkbox"] {
        width: auto;
    }

    .dynamic-editor-dialog .actions {
        display: flex;
        gap: 0.5rem;
        justify-content: flex-end;
        margin-top: 1rem;
    }
</style>
```

## That's It! 🎉

You now have a working dynamic editor. Click the button, modify the values, and click Confirm to see the data logged.

## Next Steps

### Use with View Models

```cshtml
@model EditUserViewModel

<vc:dynamic-editor model="@Model" event-name="User"></vc:dynamic-editor>
```

### Send Data to Server

```javascript
document.addEventListener('User-update', async function (e) {
    const response = await fetch('/api/users/save', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(e.detail)
    });
    
    if (response.ok) {
        alert('Saved successfully!');
    }
});
```

### Multiple Editors

```cshtml
@{
    var user = new { Name = "John" };
    var settings = new { Theme = "Dark", Language = "English" };
}

<vc:dynamic-editor model="@user" event-name="User"></vc:dynamic-editor>
<vc:dynamic-editor model="@settings" event-name="Settings"></vc:dynamic-editor>

<script>
    document.addEventListener('User-update', e => console.log('User:', e.detail));
    document.addEventListener('Settings-update', e => console.log('Settings:', e.detail));
</script>
```

## Common Issues

### Dialog doesn't open
- Ensure you're calling `showModal()` on the correct element
- Use `document.querySelector('.dynamic-editor-dialog')` to find the dialog

### Events not firing
- Check that `event-name` matches your event listener name
- Events are: `{event-name}-update` and `{event-name}-cancel`

### No fields appearing
- Complex types (objects, arrays) are skipped
- Use simple types: string, numbers, bool, DateTime, enums

## Learn More

- [DYNAMICEDITOR-README.md](DYNAMICEDITOR-README.md) - Full documentation
- [DYNAMICEDITOR-API-REFERENCE.md](DYNAMICEDITOR-API-REFERENCE.md) - API reference
- `/Home/DynamicEditor` - Live examples in the sample project

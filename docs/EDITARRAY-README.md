# EditArray TagHelper - Sample Usage Guide

> **📖 Note**: This is a Markdown file. For the best reading experience, open it in your IDE's Markdown viewer.
> If you're browsing the web application, visit `/Home/Documentation` to learn about all documentation files.

This project demonstrates how to use the `EditArrayTagHelper` from the `CK.Taghelpers` library.

## What is EditArrayTagHelper?

`EditArrayTagHelper` is an ASP.NET Core TagHelper that renders a dynamic list editor for collections (arrays/lists). It provides:

- **Display/Edit Mode Toggle**: Show items in read-only display mode with Edit/Done buttons
- **Add New Items**: Client-side addition of new items using HTML templates
- **Delete/Undelete**: Soft delete with visual feedback and undo capability
- **Reordering**: Move items up/down with automatic index renumbering
- **JavaScript Callbacks**: Hook into update and delete events
- **Model Binding**: Proper form binding for ASP.NET Core model submission

## Quick Start

### 1. Register the TagHelper

In `_ViewImports.cshtml`:

```cshtml
@addTagHelper *, CK.Taghelpers
```

### 2. Include Required JavaScript

In `_Layout.cshtml`:

```html
<script src="~/js/editArray.js"></script>
```

### 3. Create Your Model

```csharp
public record class PersonViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }  // Required for soft delete
}

public class TeamViewModel
{
    public List<PersonViewModel> Members { get; set; } = [];
}
```

### 4. Create Partial Views

**Display View** (`_PersonDisplay.cshtml`):
```cshtml
@model PersonViewModel

<div class="card mb-3 bg-light">
    <div class="card-body">
        <h5 class="card-title" data-display-for="@Html.IdFor(m => m.Name)">@Model.Name</h5>
        <h6 class="card-subtitle mb-2 text-muted" data-display-for="@Html.IdFor(m => m.Role)">@Model.Role</h6>
    </div>
</div>
```

**Edit View** (`_PersonEditor.cshtml`):
```cshtml
@model PersonViewModel

<div class="card mb-3">
    <div class="card-body">
        <input type="hidden" asp-for="Id" />
        <div class="mb-3">
            <label asp-for="Name" class="form-label">Name</label>
            <input asp-for="Name" class="form-control" />
        </div>
        <div class="mb-3">
            <label asp-for="Role" class="form-label">Role</label>
            <input asp-for="Role" class="form-control" />
        </div>
    </div>
</div>
```

### 5. Use the TagHelper

```cshtml
@model TeamViewModel

<form method="post">
    <edit-array
        id="team-members"
        asp-items="Model.Members"
        asp-for="Members"
        asp-view-name="_PersonEditor"
        asp-display-view-name="_PersonDisplay"
        asp-display-mode="true"
        asp-template="true"
        asp-add-button="true"
        asp-enable-reordering="true">
    </edit-array>

    <button type="submit" class="btn btn-primary">Save Changes</button>
</form>
```

## Attribute Reference

### Required Attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `id` | string | Unique identifier for the container. Used to generate all DOM IDs. |
| `asp-items` | IEnumerable | The collection of items to render. |
| `asp-view-name` | string | Path to the edit partial view. |
| `asp-display-view-name` | string | Path to the display partial view. |

### Optional Attributes

| Attribute | Type | Default | Description |
|-----------|------|---------|-------------|
| `asp-for` | ModelExpression | null | Model expression for proper field name generation. |
| `asp-display-mode` | bool | false | If true, items start in display mode. |
| `asp-template` | bool | false | Render a template for adding new items. |
| `asp-add-button` | bool | false | Show "Add New Item" button. |
| `asp-enable-reordering` | bool | false | Enable Move Up/Down buttons. |
| `asp-empty-placeholder` | string | null | Text shown when list is empty. |
| `asp-on-update` | string | null | JavaScript function called after Done. |
| `asp-on-delete` | string | null | JavaScript function called after Delete. |

### Styling Attributes

| Attribute | Default | Description |
|-----------|---------|-------------|
| `asp-container-class` | "edit-array-container" | CSS classes for outer container. |
| `asp-item-class` | "edit-array-item" | CSS classes for item wrappers. |
| `asp-button-class` | "btn" | Base CSS class for buttons. |
| `asp-reorder-button-class` | "btn btn-outline-secondary" | CSS classes for reorder buttons. |

### Button Text Attributes

| Attribute | Default |
|-----------|---------|
| `asp-edit-text` | "Edit" |
| `asp-delete-text` | "Delete" |
| `asp-undelete-text` | "Undelete" |
| `asp-done-text` | "Done" |
| `asp-add-text` | "Add New Item" |
| `asp-move-up-text` | "Move Up" |
| `asp-move-down-text` | "Move Down" |

## JavaScript API

The `editArray.js` file provides these global functions:

### addNewItem(containerId, templateId)
Clones the template and adds a new item to the list.

```javascript
// Called automatically by the Add button, or manually:
addNewItem('edit-array-contacts', 'edit-array-contacts-template');
```

### toggleEditMode(itemId)
Switches between display and edit mode for an item.

```javascript
toggleEditMode('edit-array-contacts-item-0');
```

### markForDeletion(itemId)
Toggles the deleted state of an item (soft delete).

```javascript
markForDeletion('edit-array-contacts-item-0');
```

### moveItem(containerId, itemId, offset)
Moves an item up (-1) or down (1) in the list.

```javascript
moveItem('edit-array-contacts', 'edit-array-contacts-item-0', -1);
```

### updateDisplayFromForm(itemId)
Updates display elements with current form values.

```javascript
updateDisplayFromForm('edit-array-contacts-item-0');
```

## Using Callbacks

Define JavaScript functions to handle update and delete events:

```cshtml
<edit-array
    asp-on-update="handleUpdate"
    asp-on-delete="handleDelete">
</edit-array>

@section Scripts {
    <script>
        function handleUpdate(itemId) {
            console.log('Item updated:', itemId);
            // Trigger AJAX save, validation, etc.
        }

        function handleDelete(itemId) {
            const item = document.getElementById(itemId);
            const isDeleted = item.getAttribute('data-deleted') === 'true';
            console.log('Delete toggled:', isDeleted);
        }
    </script>
}
```

## Display View Requirements

For the display mode to work correctly, your display partial must include `data-display-for` attributes that match the IDs of your edit inputs:

```cshtml
@model PersonViewModel

<div class="card">
    <div class="card-body">
        <!-- This span will be updated when the user edits the name -->
        <h5 data-display-for="@Html.IdFor(m => m.Name)">@Model.Name</h5>
        <p data-display-for="@Html.IdFor(m => m.Role)">@Model.Role</p>
    </div>
</div>
```

The JavaScript `updateDisplayFromForm()` function finds elements with `data-display-for` and updates their text content with the corresponding input values.

## Soft Delete Behavior

The TagHelper supports soft delete through the `IsDeleted` property:

1. If your model has an `IsDeleted` property, it will be bound automatically
2. If not, a hidden input with `data-is-deleted-marker` is added
3. When deleted, the item gets:
   - CSS class `deleted` added
   - `data-deleted="true"` attribute
   - Delete button text changes to "Undelete"
   - Edit button is disabled
   - Visual styling (via CSS)

**Important**: Newly added items (not yet saved) are completely removed from the DOM when deleted, not soft-deleted.

## Model Binding

The TagHelper generates proper field names for ASP.NET Core model binding:

```html
<!-- Without asp-for -->
<input name="[0].Name" />

<!-- With asp-for="Members" -->
<input name="Members[0].Name" />

<!-- With nested model -->
<input name="Team.Members[0].Name" />
```

When the form is submitted, ASP.NET Core will automatically bind the data back to your model, including:
- Modified items
- Deleted items (IsDeleted = true)
- Newly added items
- Reordered items (with updated indices)

## Styling

The following CSS classes are used:

- `.edit-array-container` - Outer container
- `.edit-array-items` - Items wrapper
- `.edit-array-item` - Individual item wrapper
- `.edit-array-item.deleted` - Deleted item styling
- `.display-container` - Display mode container
- `.edit-container` - Edit mode container
- `.reorder-controls` - Reorder button wrapper
- `.edit-array-placeholder` - Empty list placeholder

Example custom CSS:

```css
.edit-array-item.deleted {
    opacity: 0.5;
    background-color: #f8d7da;
}

.edit-array-item.deleted > * {
    text-decoration: line-through;
}

.reorder-controls {
    display: flex;
    gap: 0.5rem;
    margin-top: 0.5rem;
}
```

## Examples in This Project

The project includes a landing page at the root and a samples page with working examples:

- **Landing Page** (`Views/Home/Index.cshtml`) - Overview and quick navigation
- **Examples Page** (`Views/Home/Samples.cshtml`) - 5 comprehensive working examples:

Navigate to `/Home/Samples` or click "View Examples" from the landing page to see:

1. **Example 1**: Full-featured with display mode, add, reorder, and callbacks
2. **Example 2**: Basic list (edit mode only)
3. **Example 3**: Custom styling
4. **Example 4**: Empty list with placeholder
5. **Example 5**: JavaScript callbacks

## Troubleshooting

### Buttons don't work
- Ensure `editArray.js` is included in your page
- Check browser console for JavaScript errors
- Verify the script loads before the page content

### Display mode doesn't update
- Ensure your display partial has `data-display-for` attributes
- Verify the attribute values match the input IDs from the edit partial
- Check that `Html.IdFor()` is used to generate IDs

### Items aren't binding on submit
- Verify `asp-for` is set correctly
- Check that input names follow the pattern `PropertyName[index].FieldName`
- Ensure the form has `method="post"` and proper action

### Reordering breaks validation
- The JavaScript automatically updates validation attributes
- If using custom validation, ensure attributes are updated in `renumberItems()`

### New items aren't saved
- Verify your controller action accepts the model
- Check that the template renders proper input names
- Ensure `ModelState.IsValid` isn't failing

## Advanced Usage

### AJAX Save on Update

```javascript
function handleUpdate(itemId) {
    const item = document.getElementById(itemId);
    const editContainer = item.querySelector('.edit-container');
    const formData = new FormData();

    // Collect form data from edit container
    editContainer.querySelectorAll('input, select, textarea').forEach(input => {
        if (input.name) {
            formData.append(input.name, input.value);
        }
    });

    // Send to server
    fetch('/api/items/save', {
        method: 'POST',
        body: formData
    }).then(response => response.json())
      .then(data => {
          console.log('Saved:', data);
      });
}
```

### Custom Validation

```javascript
function handleUpdate(itemId) {
    const item = document.getElementById(itemId);
    const nameInput = item.querySelector('input[name$=".Name"]');

    if (!nameInput.value) {
        alert('Name is required!');
        toggleEditMode(itemId); // Switch back to edit
        return;
    }

    // Continue with normal toggle
}
```

### Confirmation Dialogs

```javascript
function handleDelete(itemId) {
    const item = document.getElementById(itemId);
    const isDeleted = item.getAttribute('data-deleted') === 'true';

    if (isDeleted) {
        if (!confirm('Really delete this item?')) {
            // Cancel deletion
            markForDeletion(itemId);
        }
    }
}
```

## License

This sample usage project is provided as-is for demonstration purposes.

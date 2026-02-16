# EditArray TagHelper - API Reference

Quick reference for all attributes and JavaScript functions.

## Required Attributes

```cshtml
<edit-array
    id="unique-id"                              <!-- Unique ID (required) -->
    asp-items="Model.List"                      <!-- Collection to render (required) -->
    asp-view-name="_Editor"                     <!-- Edit partial view (required) -->
    asp-display-view-name="_Display">           <!-- Display partial view (required) -->
</edit-array>
```

## Core Functionality

```cshtml
<!-- Model binding -->
asp-for="PropertyName"                          <!-- ModelExpression for field names -->

<!-- Display mode -->
asp-display-mode="true"                         <!-- Start in display mode (default: false) -->

<!-- Add new items -->
asp-template="true"                             <!-- Render template for new items -->
asp-add-button="true"                           <!-- Show Add button -->

<!-- Reordering -->
asp-enable-reordering="true"                    <!-- Enable Move Up/Down buttons -->

<!-- Empty state -->
asp-empty-placeholder="No items yet"            <!-- Text when list is empty -->
```

## JavaScript Callbacks

```cshtml
<!-- Event callbacks -->
asp-on-update="functionName"                    <!-- Called after Done -->
asp-on-delete="functionName"                    <!-- Called after Delete -->
```

Example:
```javascript
function functionName(itemId) {
    console.log('Item ID:', itemId);
}
```

## Styling Attributes

```cshtml
<!-- CSS classes -->
asp-container-class="border rounded"            <!-- Container classes -->
asp-item-class="mb-3"                           <!-- Item wrapper classes -->
asp-button-class="btn btn-sm"                   <!-- Button base class -->
asp-reorder-button-class="btn btn-outline"      <!-- Reorder button class -->
```

## Button Text Customization

```cshtml
asp-add-text="Add Item"                         <!-- Add button text -->
asp-edit-text="Edit"                            <!-- Edit button text -->
asp-delete-text="Delete"                        <!-- Delete button text -->
asp-undelete-text="Undelete"                    <!-- Undelete button text (when item is deleted) -->
asp-done-text="Done"                            <!-- Done button text -->
asp-move-up-text="↑"                            <!-- Move up button text -->
asp-move-down-text="↓"                          <!-- Move down button text -->
```

## JavaScript Functions

All functions are globally available from `editArray.js`.

### addNewItem(containerId, templateId, data?)
Add a new item from template.

```javascript
addNewItem('edit-array-mylist', 'edit-array-mylist-template');
```

- Clones template HTML
- Replaces `__index__` with actual index
- Updates all IDs, names, labels, validation attributes
- Shows edit container, hides display container
- Disables Add button until Done is clicked

### toggleEditMode(itemId)
Toggle between display and edit mode.

```javascript
toggleEditMode('edit-array-mylist-item-0');
```

- Switches visibility of display/edit containers
- Calls `updateDisplayFromForm()` when switching to display
- Re-enables Add button

### markForDeletion(itemId)
Toggle deleted state (soft delete).

```javascript
markForDeletion('edit-array-mylist-item-0');
```

- Adds/removes `deleted` CSS class
- Sets `data-deleted` attribute
- Changes Delete button to Undelete
- Disables Edit button when deleted
- For new items: removes from DOM entirely

### moveItem(containerId, itemId, offset)
Move item up (-1) or down (1).

```javascript
moveItem('edit-array-mylist', 'edit-array-mylist-item-0', -1);  // Move up
moveItem('edit-array-mylist', 'edit-array-mylist-item-0', 1);   // Move down
```

- Reorders DOM elements
- Calls `renumberItems()` to update indices

### updateDisplayFromForm(itemId)
Update display elements with current form values.

```javascript
updateDisplayFromForm('edit-array-mylist-item-0');
```

- Finds elements with `data-display-for` attribute
- Updates their `textContent` with corresponding input values

### renumberItems(containerId)
Renumber all items after reordering.

```javascript
renumberItems('edit-array-mylist');
```

- Updates IDs, names, labels, validation attributes
- Rewrites `onclick` handlers
- Called automatically by `moveItem()`

## HTML Structure

```html
<!-- Generated structure -->
<div class="edit-array-container" id="edit-array-{id}">
    <div class="edit-array-items" id="edit-array-{id}-items">

        <!-- Each item -->
        <div class="edit-array-item" id="edit-array-{id}-item-0">

            <!-- Display mode -->
            <div class="display-container" id="edit-array-{id}-item-0-display">
                <!-- Your display partial content -->
                <button onclick="toggleEditMode(...)">Edit</button>
                <button onclick="markForDeletion(...)">Delete</button>
            </div>

            <!-- Edit mode -->
            <div class="edit-container" id="edit-array-{id}-item-0-edit">
                <!-- Your edit partial content -->
                <button onclick="toggleEditMode(...)">Done</button>
            </div>

            <!-- Reorder buttons (if enabled) -->
            <div class="reorder-controls">
                <button onclick="moveItem(...,-1)">Move Up</button>
                <button onclick="moveItem(...,1)">Move Down</button>
            </div>
        </div>

    </div>

    <!-- Template (if enabled) -->
    <template id="edit-array-{id}-template">
        <!-- Template content with __index__ placeholders -->
    </template>

    <!-- Add button (if enabled) -->
    <button onclick="addNewItem(...)">Add New Item</button>
</div>
```

## CSS Classes

### Applied by TagHelper
- `.edit-array-container` - Outer container (always)
- `.edit-array-items` - Items wrapper (always)
- `.edit-array-item` - Item wrapper (always)
- `.display-container` - Display mode wrapper
- `.edit-container` - Edit mode wrapper
- `.edit-array-placeholder` - Empty state message
- `.reorder-controls` - Reorder buttons wrapper

### Applied by JavaScript
- `.deleted` - Added when item is marked for deletion

### Button Classes (you customize)
- `.edit-item-btn` - Edit button
- `.delete-item-btn` - Delete button
- `.done-edit-btn` - Done button
- `.reorder-btn` - Reorder buttons
- `.reorder-up-btn` - Move up button
- `.reorder-down-btn` - Move down button

## Data Attributes

### Used by TagHelper
- `data-reorder-enabled="true"` - Set on container when reordering is enabled
- `data-delete-text="{text}"` - Configurable Delete button text (set on container)
- `data-undelete-text="{text}"` - Configurable Undelete button text (set on container)
- `data-is-deleted-marker` - Marks the IsDeleted hidden input
- `data-display-for="{inputId}"` - Links display element to input for updates

### Used by JavaScript
- `data-deleted="true"` - Set on deleted items
- `data-new-item-marker="true"` - Marks newly added items
- `data-id="{value}"` - Custom ID tracking
- `data-direction="-1|1"` - Reorder delta used by `moveItem` (-1 up, 1 down)
- `data-cancel` - Marks cancel button on new items

## Model Requirements

### Minimal Model
```csharp
public class Item
{
    public bool IsDeleted { get; set; }  // Recommended for soft delete
}
```

### Complete Model
```csharp
public class Item
{
    public int Id { get; set; }                  // Optional: primary key
    public string Name { get; set; } = "";       // Your properties
    public bool IsDeleted { get; set; }          // Soft delete flag
}

public class ParentModel
{
    public List<Item> Items { get; set; } = new();
}
```

## Partial View Requirements

### Display Partial
Must include `data-display-for` for live updates:

```cshtml
@model Item

<div>
    <span data-display-for="@Html.IdFor(m => m.Name)">@Model.Name</span>
    <span data-display-for="@Html.IdFor(m => m.Email)">@Model.Email</span>
</div>
```

### Edit Partial
Use standard ASP.NET Core tag helpers:

```cshtml
@model Item

<div>
    <input type="hidden" asp-for="Id" />
    <input asp-for="Name" class="form-control" />
    <span asp-validation-for="Name" class="text-danger"></span>
</div>
```

## Form Binding

### Field Name Generation

Without `asp-for`:
```html
<input name="[0].Name" />
<input name="[1].Name" />
```

With `asp-for="Items"`:
```html
<input name="Items[0].Name" />
<input name="Items[1].Name" />
```

With nested model `asp-for="Team.Items"`:
```html
<input name="Team.Items[0].Name" />
<input name="Team.Items[1].Name" />
```

### Controller Binding

```csharp
[HttpPost]
public IActionResult Save(ParentModel model)
{
    // model.Items contains:
    // - All items (including new ones)
    // - Modified values
    // - IsDeleted flags
    // - Reordered indices

    var deletedItems = model.Items.Where(x => x.IsDeleted);
    var activeItems = model.Items.Where(x => !x.IsDeleted);

    return RedirectToAction("Index");
}
```

## jQuery Validation Integration

The TagHelper automatically integrates with jQuery Unobtrusive Validation:

```javascript
// When adding new items, validation is automatically re-parsed
// if jQuery validation is present

// Manual re-parse if needed:
var $form = $('form');
$form.removeData('validator');
$form.removeData('unobtrusiveValidator');
$.validator.unobtrusive.parse($form);
```

## Security

All user-provided values are HTML-encoded:
- IDs
- CSS classes
- Button text
- Placeholder text
- Callback function names

## Browser Compatibility

Requires:
- Modern JavaScript (ES6+)
- `document.getElementById()`
- `querySelector()`
- `closest()`
- `replaceAll()` (polyfill for older browsers)
- `template` element support

## Complete Example

```cshtml
@model TeamViewModel

<form method="post">
    <edit-array
        id="team-members"
        asp-items="Model.Members"
        asp-for="Members"
        asp-view-name="~/Views/Shared/_MemberEditor.cshtml"
        asp-display-view-name="~/Views/Shared/_MemberDisplay.cshtml"
        asp-display-mode="true"
        asp-template="true"
        asp-add-button="true"
        asp-enable-reordering="true"
        asp-empty-placeholder="No team members yet"
        asp-add-text="Add Member"
        asp-container-class="border rounded p-3"
        asp-item-class="mb-2"
        asp-on-update="handleUpdate"
        asp-on-delete="handleDelete">
    </edit-array>

    <button type="submit" class="btn btn-primary">Save</button>
</form>

@section Scripts {
    <script>
        function handleUpdate(itemId) {
            console.log('Updated:', itemId);
        }

        function handleDelete(itemId) {
            console.log('Deleted:', itemId);
        }
    </script>
}
```

## Default Values Reference

| Attribute | Default Value |
|-----------|---------------|
| `asp-display-mode` | `false` |
| `asp-template` | `false` |
| `asp-add-button` | `false` |
| `asp-enable-reordering` | `false` |
| `asp-container-class` | `"edit-array-container"` (always included) |
| `asp-item-class` | `"edit-array-item"` (always included) |
| `asp-button-class` | `"btn"` |
| `asp-reorder-button-class` | `"btn btn-outline-secondary"` |
| `asp-edit-text` | `"Edit"` |
| `asp-delete-text` | `"Delete"` |
| `asp-undelete-text` | `"Undelete"` |
| `asp-done-text` | `"Done"` |
| `asp-add-text` | `"Add New Item"` |
| `asp-move-up-text` | `"Move Up"` |
| `asp-move-down-text` | `"Move Down"` |

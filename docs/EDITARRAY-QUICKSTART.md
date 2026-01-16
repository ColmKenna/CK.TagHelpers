# EditArray TagHelper - Quick Start Guide

Get started with EditArrayTagHelper in 5 minutes!

## Minimal Example

This is the absolute minimum setup to get a working list editor.

### Step 1: Create Your Model (with IsDeleted)

```csharp
public class Person
{
    public string Name { get; set; } = "";
    public bool IsDeleted { get; set; }  // Required for delete functionality
}
```

### Step 2: Create Edit Partial View

**File**: `Views/Shared/_PersonEditor.cshtml`

```cshtml
@model Person

<div class="mb-2">
    <input asp-for="Name" class="form-control" placeholder="Name" />
</div>
```

### Step 3: Create Display Partial View

**File**: `Views/Shared/_PersonDisplay.cshtml`

```cshtml
@model Person

<div class="p-2 bg-light rounded">
    <strong data-display-for="@Html.IdFor(m => m.Name)">@Model.Name</strong>
</div>
```

### Step 4: Use in Your View

```cshtml
@model List<Person>

<form method="post">
    <edit-array
        id="people"
        asp-items="Model"
        asp-for="Model"
        asp-view-name="_PersonEditor"
        asp-display-view-name="_PersonDisplay"
        asp-display-mode="true">
    </edit-array>

    <button type="submit" class="btn btn-primary">Save</button>
</form>

@section Scripts {
    <script src="~/js/editArray.js"></script>
}
```

### Step 5: Setup Controller

```csharp
public class HomeController : Controller
{
    public IActionResult Index()
    {
        var people = new List<Person>
        {
            new() { Name = "Alice" },
            new() { Name = "Bob" }
        };
        return View(people);
    }

    [HttpPost]
    public IActionResult Index(List<Person> model)
    {
        // model will contain all items, including:
        // - Modified items
        // - Items with IsDeleted = true
        // - New items (if you enabled add button)

        return View(model);
    }
}
```

That's it! You now have a working list editor with display/edit toggle and delete functionality.

## Add More Features

### Enable Adding New Items

```cshtml
<edit-array
    id="people"
    asp-items="Model"
    asp-for="Model"
    asp-view-name="_PersonEditor"
    asp-display-view-name="_PersonDisplay"
    asp-display-mode="true"
    asp-template="true"
    asp-add-button="true"
    asp-add-text="Add Person">
</edit-array>
```

### Enable Reordering

```cshtml
<edit-array
    id="people"
    asp-items="Model"
    asp-for="Model"
    asp-view-name="_PersonEditor"
    asp-display-view-name="_PersonDisplay"
    asp-display-mode="true"
    asp-enable-reordering="true"
    asp-move-up-text="↑"
    asp-move-down-text="↓">
</edit-array>
```

### Show Placeholder When Empty

```cshtml
<edit-array
    id="people"
    asp-items="Model"
    asp-for="Model"
    asp-view-name="_PersonEditor"
    asp-display-view-name="_PersonDisplay"
    asp-display-mode="true"
    asp-empty-placeholder="No people added yet.">
</edit-array>
```

### Add Callbacks

```cshtml
<edit-array
    id="people"
    asp-items="Model"
    asp-for="Model"
    asp-view-name="_PersonEditor"
    asp-display-view-name="_PersonDisplay"
    asp-display-mode="true"
    asp-on-update="handleSave"
    asp-on-delete="handleDelete">
</edit-array>

@section Scripts {
    <script>
        function handleSave(itemId) {
            console.log('Saved:', itemId);
        }

        function handleDelete(itemId) {
            console.log('Deleted:', itemId);
        }
    </script>
}
```

## All Features Combined

```cshtml
<edit-array
    id="people"
    asp-items="Model"
    asp-for="Model"
    asp-view-name="_PersonEditor"
    asp-display-view-name="_PersonDisplay"
    asp-display-mode="true"
    asp-template="true"
    asp-add-button="true"
    asp-enable-reordering="true"
    asp-empty-placeholder="No people yet"
    asp-add-text="Add Person"
    asp-edit-text="Edit"
    asp-delete-text="Delete"
    asp-done-text="Done"
    asp-move-up-text="↑"
    asp-move-down-text="↓"
    asp-on-update="handleSave"
    asp-on-delete="handleDelete">
</edit-array>
```

## Important Notes

### ⚠️ The `data-display-for` Attribute

Your display partial **must** include `data-display-for` attributes to enable live updates:

```cshtml
<!-- ❌ Wrong - won't update when editing -->
<strong>@Model.Name</strong>

<!-- ✅ Correct - will update when editing -->
<strong data-display-for="@Html.IdFor(m => m.Name)">@Model.Name</strong>
```

### ⚠️ The IsDeleted Property

Your model should have an `IsDeleted` property for soft delete:

```csharp
public class Person
{
    public string Name { get; set; } = "";
    public bool IsDeleted { get; set; }  // This enables soft delete
}
```

If your model doesn't have this property, the TagHelper will add a hidden input automatically.

### ⚠️ JavaScript Dependency

The `editArray.js` file must be included on pages using the TagHelper:

```html
<script src="~/js/editArray.js"></script>
```

You can include it globally in `_Layout.cshtml` or per-page in a `@section Scripts` block.

## Common Patterns

### Read-Only Mode (No Delete/Edit)

If you just want to display items without editing:

```cshtml
<edit-array
    id="people"
    asp-items="Model"
    asp-view-name="_PersonDisplay"
    asp-display-view-name="_PersonDisplay"
    asp-display-mode="true">
</edit-array>
```

Note: Both view names point to the display partial, and no template/add buttons are enabled.

### Always-Editable (No Display Mode)

If you want items always in edit mode:

```cshtml
<edit-array
    id="people"
    asp-items="Model"
    asp-view-name="_PersonEditor"
    asp-display-view-name="_PersonDisplay"
    asp-display-mode="false">
</edit-array>
```

### Nested Models

For complex models with nested properties:

```csharp
public class Team
{
    public string Name { get; set; } = "";
    public List<Person> Members { get; set; } = new();
}
```

```cshtml
@model Team

<input asp-for="Name" class="form-control mb-3" />

<edit-array
    id="members"
    asp-items="Model.Members"
    asp-for="Members"
    asp-view-name="_PersonEditor"
    asp-display-view-name="_PersonDisplay"
    asp-display-mode="true">
</edit-array>
```

The `asp-for="Members"` ensures proper field names like `Members[0].Name`.

## Styling Tips

Add custom CSS classes:

```cshtml
<edit-array
    id="people"
    asp-items="Model"
    asp-view-name="_PersonEditor"
    asp-display-view-name="_PersonDisplay"
    asp-display-mode="true"
    asp-container-class="border rounded p-3 shadow-sm"
    asp-item-class="mb-3"
    asp-button-class="btn btn-sm">
</edit-array>
```

Default classes that always get added:
- Container: `edit-array-container` (always present)
- Item: `edit-array-item` (always present)

## Next Steps

- See [README.md](README.md) for complete documentation
- Run the sample project to see all examples in action
- Check `Views/Home/Index.cshtml` for 5 comprehensive examples

## Troubleshooting

**Buttons don't work**: Make sure `editArray.js` is included

**Display doesn't update**: Add `data-display-for` attributes to your display partial

**Form doesn't submit data**: Check that `asp-for` is set and form has `method="post"`

**Validation errors**: Ensure your model properties have appropriate validation attributes

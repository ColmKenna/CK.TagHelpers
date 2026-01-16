# EditArray TagHelper - Troubleshooting Guide

Common issues and their solutions.

## Table of Contents

- [Buttons Don't Work](#buttons-dont-work)
- [Display Mode Issues](#display-mode-issues)
- [Form Submission Problems](#form-submission-problems)
- [Validation Issues](#validation-issues)
- [Reordering Problems](#reordering-problems)
- [Add New Item Issues](#add-new-item-issues)
- [Delete/Undelete Problems](#deleteundelete-problems)
- [Styling Issues](#styling-issues)
- [Performance Issues](#performance-issues)

---

## Buttons Don't Work

### Symptom
Clicking Edit, Delete, Done, or Add buttons does nothing.

### Causes & Solutions

#### ❌ JavaScript file not included
```html
<!-- Missing this in _Layout.cshtml or view -->
<script src="~/js/editArray.js"></script>
```

**Solution**: Add the script reference before `</body>` or in a Scripts section.

#### ❌ Script loaded after content
If using `@section Scripts`, make sure `editArray.js` loads before your page-specific scripts:

```cshtml
@section Scripts {
    <script src="~/js/editArray.js"></script>  <!-- Load first -->
    <script>
        // Your callbacks
    </script>
}
```

**Better solution**: Include `editArray.js` globally in `_Layout.cshtml`.

#### ❌ JavaScript errors
**Check browser console** (F12) for errors.

Common errors:
- `Uncaught ReferenceError: toggleEditMode is not defined`
  - Solution: Include editArray.js
- `Cannot read property 'querySelector' of null`
  - Solution: Check that element IDs match

#### ❌ CSP (Content Security Policy) blocking inline scripts
The TagHelper uses inline `onclick` handlers. If you have strict CSP:

**Solution**: Add `unsafe-inline` to your CSP policy or rewrite to use event delegation.

---

## Display Mode Issues

### Symptom
Display mode doesn't update when clicking Done.

### Causes & Solutions

#### ❌ Missing `data-display-for` attributes

**Wrong**:
```cshtml
@model PersonViewModel
<div>
    <strong>@Model.Name</strong>  <!-- Won't update -->
</div>
```

**Correct**:
```cshtml
@model PersonViewModel
<div>
    <strong data-display-for="@Html.IdFor(m => m.Name)">@Model.Name</strong>
</div>
```

#### ❌ ID mismatch

The `data-display-for` value must **exactly match** the input ID in the edit partial.

**Edit partial**:
```cshtml
<input asp-for="Name" />  <!-- Generates id="Name" -->
```

**Display partial**:
```cshtml
<!-- Must match the generated ID -->
<span data-display-for="Name">@Model.Name</span>

<!-- Or use Html.IdFor to generate it correctly -->
<span data-display-for="@Html.IdFor(m => m.Name)">@Model.Name</span>
```

#### ❌ Display mode not enabled

```cshtml
<!-- Missing asp-display-mode="true" -->
<edit-array
    asp-display-mode="true"  <!-- Add this -->
    ...>
</edit-array>
```

---

## Form Submission Problems

### Symptom
Data doesn't submit or model is null/empty.

### Causes & Solutions

#### ❌ Missing `asp-for`

**Wrong**:
```cshtml
<edit-array
    asp-items="Model.Members"
    asp-view-name="_Editor"
    asp-display-view-name="_Display">
</edit-array>
```

**Correct**:
```cshtml
<edit-array
    asp-items="Model.Members"
    asp-for="Members"  <!-- Add this for proper binding -->
    asp-view-name="_Editor"
    asp-display-view-name="_Display">
</edit-array>
```

#### ❌ Form missing method="post"

```html
<form asp-action="Index" method="post">  <!-- Add method="post" -->
```

#### ❌ Controller parameter doesn't match

**View**:
```cshtml
asp-for="Members"
```

**Controller** (Wrong):
```csharp
[HttpPost]
public IActionResult Index(List<PersonViewModel> items)  // Wrong name
```

**Controller** (Correct):
```csharp
[HttpPost]
public IActionResult Index(TeamViewModel model)  // Matches view model
{
    // model.Members will be populated
}
```

#### ❌ Collection binding not working

Check generated field names in browser DevTools:

**Expected**:
```html
<input name="Members[0].Name" />
<input name="Members[1].Name" />
```

**If you see**:
```html
<input name="[0].Name" />  <!-- Missing prefix -->
```

**Solution**: Add `asp-for="Members"` to the edit-array tag.

---

## Validation Issues

### Symptom
Validation doesn't work or shows errors incorrectly.

### Causes & Solutions

#### ❌ Validation scripts not included

```cshtml
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script src="~/js/editArray.js"></script>
}
```

#### ❌ Validation not re-parsed after adding items

The TagHelper automatically re-parses validation if jQuery is present. If it's not working:

**Manual re-parse**:
```javascript
function revalidateForm() {
    var $form = $('form');
    $form.removeData('validator');
    $form.removeData('unobtrusiveValidator');
    $.validator.unobtrusive.parse($form);
}
```

#### ❌ Validation messages not showing

Ensure your edit partial includes validation spans:

```cshtml
<input asp-for="Name" class="form-control" />
<span asp-validation-for="Name" class="text-danger"></span>
```

#### ❌ Validation on reordered items

After reordering, validation attributes are updated automatically. If issues persist:

```javascript
// After moveItem, manually re-parse
moveItem(containerId, itemId, offset);
revalidateForm();  // From above
```

---

## Reordering Problems

### Symptom
Reorder buttons don't appear or reordering breaks validation.

### Causes & Solutions

#### ❌ Reordering not enabled

```cshtml
<edit-array
    asp-enable-reordering="true"  <!-- Add this -->
    ...>
</edit-array>
```

#### ❌ Move buttons don't work at boundaries

This is **expected behavior**:
- First item: Move Up button does nothing
- Last item: Move Down button does nothing

The JavaScript checks bounds and returns early if out of range.

#### ❌ Reordering breaks custom attributes

The `renumberItems()` function updates these attributes:
- `id`
- `name`
- `for`
- `data-id`
- `data-display-for`
- `data-valmsg-for`
- `aria-describedby`
- `onclick` handlers

If you have **custom attributes** with indices, they won't be updated.

**Solution**: Modify `renumberItems()` in editArray.js to include your custom attributes.

---

## Add New Item Issues

### Symptom
Add button doesn't appear or doesn't work.

### Causes & Solutions

#### ❌ Template or Add button not enabled

```cshtml
<edit-array
    asp-template="true"     <!-- Both required -->
    asp-add-button="true"   <!-- for Add button -->
    ...>
</edit-array>
```

#### ❌ Add button stays disabled

This is **expected** after clicking Add. The button is disabled until you:
- Click Done (saves the new item)
- Click Cancel/Delete (removes the new item)

**Why?** This prevents adding multiple incomplete items.

#### ❌ New item has wrong field names

Check that template fields use `__index__`:

**Browser DevTools** (template before cloning):
```html
<input name="Members[__index__].Name" />
```

**After clicking Add** (should become):
```html
<input name="Members[2].Name" />  <!-- __index__ replaced with 2 -->
```

If `__index__` isn't being replaced:
- Check that your model uses proper `asp-for` binding
- Verify the template is being generated correctly

#### ❌ New items don't validate

Ensure jQuery Unobtrusive Validation is loaded:

```cshtml
@section Scripts {
    <partial name="_ValidationScriptsPartial" />  <!-- Must be loaded -->
    <script src="~/js/editArray.js"></script>
}
```

---

## Delete/Undelete Problems

### Symptom
Delete doesn't work or items aren't marked deleted.

### Causes & Solutions

#### ❌ IsDeleted property missing

**Model without IsDeleted**:
```csharp
public class Person
{
    public string Name { get; set; }
    // No IsDeleted property
}
```

**Solution**: Add the property:
```csharp
public class Person
{
    public string Name { get; set; }
    public bool IsDeleted { get; set; }  // Add this
}
```

**Alternative**: The TagHelper will add a hidden input automatically, but it's better to have the property.

#### ❌ Deleted items not saved

Deleted items are marked with `IsDeleted = true` but **not removed** from the collection.

**Controller**:
```csharp
[HttpPost]
public IActionResult Save(TeamViewModel model)
{
    // Deleted items are still in the collection
    var deleted = model.Members.Where(m => m.IsDeleted).ToList();
    var active = model.Members.Where(m => !m.IsDeleted).ToList();

    // Remove deleted items from database
    foreach (var item in deleted)
    {
        _context.Remove(item);
    }

    _context.SaveChanges();
    return RedirectToAction("Index");
}
```

#### ❌ Delete button shows wrong text

The JavaScript automatically changes button text:
- Not deleted: "Delete"
- Deleted: "Undelete"

If this isn't happening:
- Check that `editArray.js` is loaded
- Check browser console for errors
- Verify the button has class `delete-item-btn`

#### ❌ Newly added items don't delete properly

**Expected behavior**: Newly added items (not yet saved) are **completely removed** from the DOM when deleted.

Existing items are **soft deleted** (marked with IsDeleted = true).

---

## Styling Issues

### Symptom
Styling doesn't look right or deleted items look wrong.

### Causes & Solutions

#### ❌ Missing deleted item styles

Add CSS for deleted items:

```css
.edit-array-item.deleted {
    opacity: 0.5;
    background-color: #f8d7da;
}

.edit-array-item.deleted > * {
    text-decoration: line-through;
}
```

#### ❌ Custom classes not applied

```cshtml
<edit-array
    asp-container-class="my-custom-class"  <!-- Your class -->
    ...>
</edit-array>
```

**Result**:
```html
<!-- Both classes are applied -->
<div class="edit-array-container my-custom-class">
```

The default classes (`edit-array-container`, `edit-array-item`) are **always included**.

#### ❌ Buttons have wrong styles

```cshtml
<edit-array
    asp-button-class="btn btn-sm"  <!-- Base class for all buttons -->
    ...>
</edit-array>
```

Button-specific classes are appended:
- Edit: `btn btn-sm btn-primary edit-item-btn`
- Delete: `btn btn-sm btn-danger delete-item-btn`
- Done: `btn btn-sm btn-success done-edit-btn`

---

## Performance Issues

### Symptom
Slow rendering or laggy interactions with many items.

### Solutions

#### ❌ Too many items

If rendering 100+ items:

1. **Use pagination** or **infinite scroll** instead of rendering all items
2. **Disable display mode** (always show edit mode) to reduce DOM size
3. **Simplify partials** - remove unnecessary markup

#### ❌ Complex partials

Keep your edit/display partials simple:

**Bad** (complex):
```cshtml
@model Person
<div class="card shadow-lg">
    <div class="card-header bg-gradient">...</div>
    <div class="card-body">
        <!-- Lots of nested divs and Bootstrap components -->
    </div>
</div>
```

**Good** (simple):
```cshtml
@model Person
<div class="p-2 border">
    <input asp-for="Name" class="form-control" />
</div>
```

#### ❌ Validation on every keystroke

If you have custom validation that runs on every keystroke, it can be slow with many items.

**Solution**: Debounce validation or validate only on blur.

---

## Common Error Messages

### "Cannot read property 'querySelector' of null"

**Cause**: Element ID doesn't exist.

**Solution**: Check that the `id` attribute matches what JavaScript expects:
- Container: `edit-array-{yourid}`
- Items container: `edit-array-{yourid}-items`
- Item: `edit-array-{yourid}-item-{index}`

### "Uncaught ReferenceError: toggleEditMode is not defined"

**Cause**: `editArray.js` not loaded.

**Solution**: Add `<script src="~/js/editArray.js"></script>`

### "Object doesn't support property or method 'closest'"

**Cause**: Old browser (IE11).

**Solution**: Add a polyfill for `closest()` or use a modern browser.

### Form submits but model is null

**Cause**: Model binding issue.

**Solutions**:
1. Add `asp-for="PropertyName"` to edit-array
2. Check controller parameter name matches property name
3. Verify form has `method="post"`
4. Check field names in browser DevTools

### Items render but have no buttons

**Cause**: Display mode settings issue.

**Solution**: Check `asp-display-mode` value:
- `true`: Shows display container with Edit/Delete buttons
- `false`: Shows only edit container (no buttons by default)

---

## Debug Checklist

When things don't work, check this list:

- [ ] `editArray.js` is included and loads without errors
- [ ] Browser console shows no JavaScript errors (F12)
- [ ] Element IDs are generated correctly (inspect with DevTools)
- [ ] `asp-for` is set on the edit-array tag
- [ ] Display partial has `data-display-for` attributes
- [ ] Edit partial has proper `asp-for` bindings
- [ ] Model has `IsDeleted` property
- [ ] Form has `method="post"`
- [ ] Controller parameter matches model structure
- [ ] TagHelper is registered in `_ViewImports.cshtml`

---

## Getting More Help

1. **Check the generated HTML**: Use browser DevTools to inspect the actual HTML output
2. **Check browser console**: Look for JavaScript errors
3. **Check network tab**: Verify form posts are working
4. **Enable debug mode**: Add `console.log()` statements in editArray.js
5. **Review examples**: Compare your code with the working examples in `Views/Home/Index.cshtml`

## Logging for Debugging

Add this to your page to see what's happening:

```javascript
// Override functions to add logging
const originalToggle = window.toggleEditMode;
window.toggleEditMode = function(itemId) {
    console.log('toggleEditMode called:', itemId);
    return originalToggle(itemId);
};

const originalDelete = window.markForDeletion;
window.markForDeletion = function(itemId) {
    console.log('markForDeletion called:', itemId);
    return originalDelete(itemId);
};

// Log form submission
document.querySelector('form').addEventListener('submit', function(e) {
    const formData = new FormData(this);
    console.log('Form submitting with:');
    for (let [key, value] of formData.entries()) {
        console.log(`  ${key}: ${value}`);
    }
});
```

This will help you see exactly what's being called and what data is being submitted.

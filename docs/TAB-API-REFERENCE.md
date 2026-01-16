# Tab TagHelper - API Reference

Quick reference for all Tab TagHelper attributes, CSS classes, and generated HTML structure.

## `<tab>` Element

Container for tab items. Renders as `<div class="tabs">`.

### Attributes

| Attribute | Type | Default | Description |
|-----------|------|---------|-------------|
| `class` | string | `"tabs"` | CSS classes for the container. User classes are merged with `tabs`. |

### Behavior

- Generates a unique radio button group name for child tab items
- Tracks used IDs to ensure uniqueness within the group
- Auto-selects the first tab if no tab has `selected="true"`

### Example

```cshtml
<tab>
    <!-- tab-item children here -->
</tab>

<tab class="shadow-lg rounded">
    <!-- additional classes are merged -->
</tab>
```

---

## `<tab-item>` Element

Individual tab with heading and content. Must be a direct child of `<tab>`.

### Attributes

| Attribute | Type | Default | Required | Description |
|-----------|------|---------|----------|-------------|
| `heading` | string | — | **Yes** | The text displayed on the tab button. |
| `id` | string | auto-generated | No | Unique identifier for the tab. Generated from heading if not provided. |
| `selected` | bool | `false` | No | When `true`, this tab is active by default. |

### ID Generation Rules

When `id` is not provided:
1. Heading text is converted to lowercase
2. Special characters are removed (keeps letters, numbers, spaces, hyphens)
3. Spaces are replaced with hyphens
4. If the ID already exists in the tab group, a numeric suffix is added (`-1`, `-2`, etc.)

**Examples**:
- `"Overview"` → `overview`
- `"User Settings"` → `user-settings`
- `"Tab #1!"` → `tab-1`
- `"Overview"` (duplicate) → `overview-1`

### Missing Heading Behavior

If `heading` is missing or empty, the tag helper renders:
```html
<!-- TabItemTagHelper: Missing required 'heading' attribute -->
```

### Example

```cshtml
<tab-item heading="Overview">
    Content here
</tab-item>

<tab-item id="custom-id" heading="Settings" selected="true">
    Pre-selected tab with custom ID
</tab-item>
```

---

## Generated HTML Structure

### Single Tab Item

Input:
```cshtml
<tab-item heading="Overview">Content here</tab-item>
```

Output:
```html
<input class="tabs-panel-input" 
       name="tabs-{groupId}" 
       type="radio" 
       id="overview" 
       role="tab" 
       aria-controls="overview-panel" 
       checked="checked" />
<label class="tab-heading" for="overview">Overview</label>
<div class="panel" id="overview-panel" role="tabpanel" aria-labelledby="overview">
    <div class="panel-content">Content here</div>
</div>
```

### Complete Tab Group

Input:
```cshtml
<tab>
    <tab-item heading="Tab 1">First content</tab-item>
    <tab-item heading="Tab 2">Second content</tab-item>
</tab>
```

Output:
```html
<div class="tabs">
    <input class="tabs-panel-input" name="tabs-abc123" type="radio" 
           id="tab-1" role="tab" aria-controls="tab-1-panel" checked="checked" />
    <label class="tab-heading" for="tab-1">Tab 1</label>
    <div class="panel" id="tab-1-panel" role="tabpanel" aria-labelledby="tab-1">
        <div class="panel-content">First content</div>
    </div>
    
    <input class="tabs-panel-input" name="tabs-abc123" type="radio" 
           id="tab-2" role="tab" aria-controls="tab-2-panel" />
    <label class="tab-heading" for="tab-2">Tab 2</label>
    <div class="panel" id="tab-2-panel" role="tabpanel" aria-labelledby="tab-2">
        <div class="panel-content">Second content</div>
    </div>
</div>
```

---

## CSS Classes Reference

### Container Classes

| Class | Element | Description |
|-------|---------|-------------|
| `.tabs` | `<div>` | Main container. Uses flexbox with flex-wrap. |

### Tab Element Classes

| Class | Element | Description |
|-------|---------|-------------|
| `.tabs-panel-input` | `<input type="radio">` | Hidden radio button for state. Position absolute, opacity 0. |
| `.tab-heading` | `<label>` | Clickable tab button. Full width on mobile, auto width on desktop. |
| `.panel` | `<div>` | Content panel. Hidden by default, shown when radio is checked. |
| `.panel-content` | `<div>` | Inner wrapper with padding and scroll. |

### State-Based Selectors

| Selector | Description |
|----------|-------------|
| `.tabs-panel-input:checked + .tab-heading` | Selected tab heading style |
| `.tabs-panel-input:checked + .tab-heading + .panel` | Shows the panel for selected tab |
| `.tabs-panel-input:focus + .tab-heading` | Tab heading when input has focus |
| `.tab-heading:hover` | Tab heading on mouse hover |
| `.tab-heading:active` | Tab heading when being clicked |

---

## ARIA Attributes Reference

| Attribute | Element | Value | Purpose |
|-----------|---------|-------|---------|
| `role="tab"` | `<input>` | Fixed | Identifies as a tab control |
| `role="tabpanel"` | `.panel` | Fixed | Identifies as a tab content area |
| `aria-controls` | `<input>` | `{id}-panel` | References the associated panel |
| `aria-labelledby` | `.panel` | `{id}` | References the tab that labels this panel |

---

## Default Values Summary

| Property | Default Value |
|----------|---------------|
| Container class | `"tabs"` |
| Tab selected | `false` (first tab auto-selected if none specified) |
| Panel height | `450px` (from CSS) |
| Responsive breakpoint | `600px` (horizontal tabs above this width) |

---

## CSS Customization Examples

### Change Tab Colors

```css
.tabs .tab-heading {
    background: #f1f5f9;
    color: #475569;
}

.tabs .tabs-panel-input:checked + .tab-heading {
    background: #3b82f6;
    color: white;
}
```

### Change Panel Height

```css
.tabs .panel {
    height: auto;
    min-height: 300px;
    max-height: 600px;
}
```

### Rounded Corners

```css
.tabs {
    border-radius: 12px;
    overflow: hidden;
}

.tabs .tab-heading:first-of-type {
    border-top-left-radius: 12px;
}

.tabs .panel:last-of-type {
    border-bottom-left-radius: 12px;
    border-bottom-right-radius: 12px;
}
```

### Remove Border

```css
.tabs {
    border: none;
}

.tabs .panel .panel-content {
    border-top: none;
}
```

---

## Integration Examples

### With Bootstrap

```cshtml
<div class="card">
    <div class="card-body">
        <tab>
            <tab-item heading="Info">
                <div class="alert alert-info">Bootstrap alert inside tab</div>
            </tab-item>
            <tab-item heading="Form">
                <div class="mb-3">
                    <label class="form-label">Name</label>
                    <input type="text" class="form-control" />
                </div>
                <button class="btn btn-primary">Submit</button>
            </tab-item>
        </tab>
    </div>
</div>
```

### With ASP.NET Core Tag Helpers

```cshtml
<tab>
    <tab-item heading="Editor">
        <div class="mb-3">
            <label asp-for="Name" class="form-label"></label>
            <input asp-for="Name" class="form-control" />
            <span asp-validation-for="Name" class="text-danger"></span>
        </div>
    </tab-item>
    <tab-item heading="Preview">
        <p>@Model.Name</p>
    </tab-item>
</tab>
```

### Dynamic Tabs with Razor Loop

```cshtml
<tab>
    @for (int i = 0; i < Model.Items.Count; i++)
    {
        <tab-item heading="@Model.Items[i].Title" 
                  id="@($"item-{Model.Items[i].Id}")"
                  selected="@(i == 0)">
            <h4>@Model.Items[i].Title</h4>
            <p>@Model.Items[i].Description</p>
        </tab-item>
    }
</tab>
```

---

## Related Documentation

- [TAB-QUICKSTART.md](TAB-QUICKSTART.md) - Get started quickly
- [TAB-README.md](TAB-README.md) - Complete documentation
- `/Home/Tabs` - Live examples in the sample application

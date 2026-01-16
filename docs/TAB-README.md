# Tab TagHelper - Complete Documentation

> **📖 Note**: This is a Markdown file. For the best reading experience, open it in your IDE's Markdown viewer.
> If you're browsing the web application, visit `/Home/Documentation` to learn about all documentation files.

This document provides comprehensive documentation for the `TabTagHelper` and `TabItemTagHelper` from the `CK.Taghelpers` library.

## What is the Tab TagHelper?

The Tab TagHelper provides a simple, declarative way to create accessible, CSS-only tabbed interfaces in ASP.NET Core Razor views. It generates semantic HTML with proper ARIA attributes for accessibility, without requiring any JavaScript.

### Key Features

- **CSS-Only Implementation**: No JavaScript required - tabs work using HTML radio buttons and CSS
- **Accessible by Default**: Generates proper ARIA attributes (`role`, `aria-controls`, `aria-labelledby`)
- **Automatic ID Generation**: Tab IDs are auto-generated from heading text if not provided
- **Auto-Selection**: The first tab is automatically selected if no tab has `selected="true"`
- **Duplicate ID Handling**: Automatically ensures unique IDs even with duplicate headings
- **Responsive Design**: Included CSS adapts to different screen sizes
- **Semantic HTML**: Renders clean, semantic HTML structure

## Quick Start

### 1. Register the TagHelper

In `_ViewImports.cshtml`:

```cshtml
@addTagHelper *, CK.Taghelpers
```

### 2. Include the CSS

In your view or `_Layout.cshtml`:

```html
<link rel="stylesheet" href="~/_content/CK.Taghelpers/css/tabs.css" />
```

### 3. Use the Tags

```cshtml
<tab>
    <tab-item heading="Overview">
        <p>Content for the overview tab.</p>
    </tab-item>
    <tab-item heading="Details">
        <p>Content for the details tab.</p>
    </tab-item>
</tab>
```

## Element Reference

### `<tab>` Element

The container element for tab items. Renders as a `<div class="tabs">`.

| Attribute | Type | Description |
|-----------|------|-------------|
| `class` | string | Optional. Additional CSS classes to merge with the default `tabs` class. |

**Behavior**:
- Creates a unique radio button group name for its child tab items
- Tracks used IDs to ensure uniqueness
- If no child has `selected="true"`, automatically selects the first tab
- Existing `class` attribute values are preserved and merged with `tabs`

### `<tab-item>` Element

Represents a single tab with its heading and content panel.

| Attribute | Type | Default | Description |
|-----------|------|---------|-------------|
| `heading` | string | **required** | The text displayed on the tab button. |
| `id` | string | auto-generated | Unique identifier for the tab. If not provided, generated from heading. |
| `selected` | bool | `false` | When `true`, this tab is active by default. |

**Behavior**:
- Must be a direct child of `<tab>`
- Renders a hidden radio input, a label, and a content panel
- ID is sanitized from heading (lowercase, special chars removed, spaces become hyphens)
- If ID conflicts with another tab in the same group, a suffix is added automatically

## Generated HTML Structure

For a basic tab:

```cshtml
<tab>
    <tab-item heading="Overview">Content here</tab-item>
    <tab-item heading="Details">More content</tab-item>
</tab>
```

The TagHelper generates:

```html
<div class="tabs">
    <!-- Tab 1: Overview -->
    <input class="tabs-panel-input" name="tabs-{uniqueId}" type="radio" 
           id="overview" role="tab" aria-controls="overview-panel" checked="checked" />
    <label class="tab-heading" for="overview">Overview</label>
    <div class="panel" id="overview-panel" role="tabpanel" aria-labelledby="overview">
        <div class="panel-content">Content here</div>
    </div>
    
    <!-- Tab 2: Details -->
    <input class="tabs-panel-input" name="tabs-{uniqueId}" type="radio" 
           id="details" role="tab" aria-controls="details-panel" />
    <label class="tab-heading" for="details">Details</label>
    <div class="panel" id="details-panel" role="tabpanel" aria-labelledby="details">
        <div class="panel-content">More content</div>
    </div>
</div>
```

## CSS Classes Reference

| Class | Element | Description |
|-------|---------|-------------|
| `.tabs` | Container div | Main wrapper, uses flexbox layout |
| `.tabs-panel-input` | Radio input | Hidden radio button for state management |
| `.tab-heading` | Label | The clickable tab button |
| `.panel` | Content div | Tab content panel (hidden by default) |
| `.panel-content` | Inner div | Padding and scroll container for content |

## Accessibility Features

The Tab TagHelper generates accessible markup following WAI-ARIA best practices:

### ARIA Attributes

| Attribute | Element | Purpose |
|-----------|---------|---------|
| `role="tab"` | Radio input | Identifies the element as a tab |
| `role="tabpanel"` | Panel div | Identifies the content area |
| `aria-controls` | Radio input | References the associated panel ID |
| `aria-labelledby` | Panel div | References the tab input ID |

### Keyboard Navigation

- **Tab/Shift+Tab**: Move focus between tabs
- **Enter/Space**: Activate the focused tab (native radio button behavior)

### Screen Reader Support

The semantic structure and ARIA attributes ensure screen readers can announce:
- Which tab is currently selected
- How many tabs exist
- The relationship between tabs and their content

## Styling and Customization

### Using the Default CSS

The included `tabs.css` provides a responsive, modern design:

- Mobile: Tabs stack vertically (accordion-style)
- Desktop (600px+): Tabs display horizontally

### Custom Styling

Override the default styles by targeting the CSS classes:

```css
/* Custom tab heading style */
.tabs .tab-heading {
    background: #2563eb;
    color: white;
    border-radius: 8px 8px 0 0;
}

/* Custom selected state */
.tabs .tabs-panel-input:checked + .tab-heading {
    background: #1d4ed8;
    border-bottom: 3px solid #fbbf24;
}

/* Custom panel style */
.tabs .panel {
    background: #f8fafc;
    border-radius: 0 0 8px 8px;
}

/* Custom panel height */
.tabs .panel {
    height: auto;
    min-height: 200px;
}
```

### Adding Custom Classes

You can add custom classes to the tab container:

```cshtml
<tab class="my-custom-tabs shadow-lg">
    <tab-item heading="Tab 1">Content</tab-item>
</tab>
```

This will render as `<div class="tabs my-custom-tabs shadow-lg">`.

## Common Patterns

### Pre-Selected Tab

```cshtml
<tab>
    <tab-item heading="First">Content</tab-item>
    <tab-item heading="Second" selected="true">Pre-selected!</tab-item>
    <tab-item heading="Third">Content</tab-item>
</tab>
```

### Custom IDs for Programmatic Access

```cshtml
<tab>
    <tab-item id="user-profile" heading="Profile">...</tab-item>
    <tab-item id="user-settings" heading="Settings">...</tab-item>
</tab>
```

### Forms in Tabs

```cshtml
<form method="post">
    <tab>
        <tab-item heading="Personal Info">
            <div class="mb-3">
                <label asp-for="Name" class="form-label"></label>
                <input asp-for="Name" class="form-control" />
            </div>
        </tab-item>
        <tab-item heading="Contact Info">
            <div class="mb-3">
                <label asp-for="Email" class="form-label"></label>
                <input asp-for="Email" class="form-control" />
            </div>
        </tab-item>
    </tab>
    <button type="submit" class="btn btn-primary mt-3">Save</button>
</form>
```

### Dynamic Tab Content with Razor

```cshtml
<tab>
    @foreach (var category in Model.Categories)
    {
        <tab-item heading="@category.Name" id="@($"category-{category.Id}")">
            <h4>@category.Name</h4>
            <p>@category.Description</p>
            <ul>
                @foreach (var item in category.Items)
                {
                    <li>@item.Name</li>
                }
            </ul>
        </tab-item>
    }
</tab>
```

## Troubleshooting

### Tabs Don't Display

- **Verify CSS is included**: Check that `tabs.css` is properly linked
- **Check browser console**: Look for 404 errors on the CSS file
- **Verify path**: Use `~/_content/CK.Taghelpers/css/tabs.css`

### Content Not Visible

- **Check tab structure**: `<tab-item>` must be a direct child of `<tab>`
- **Verify heading attribute**: Every `<tab-item>` needs a `heading` attribute
- **Inspect HTML**: Use browser dev tools to verify the generated structure

### Wrong Tab Selected

- **Check `selected` attributes**: Only one tab should have `selected="true"`
- **Verify attribute spelling**: Use `selected="true"`, not `selected="selected"`

### Styling Issues

- **CSS specificity**: Your custom styles may need higher specificity
- **Conflicting styles**: Check for CSS that overrides the tab styles
- **Missing viewport meta**: Responsive styles require proper viewport meta tag

### Duplicate IDs

The TagHelper automatically handles duplicate headings by adding numeric suffixes:
- "Overview" → `overview`
- "Overview" (second) → `overview-1`
- "Overview" (third) → `overview-2`

If you need specific IDs, provide them explicitly using the `id` attribute.

## Best Practices

1. **Use descriptive headings**: Tab headings should clearly indicate the content
2. **Keep content focused**: Each tab should have a distinct purpose
3. **Consider mobile**: Test that tabs work well on smaller screens
4. **Don't nest tabs**: Avoid putting tabs inside tabs
5. **Limit tab count**: Too many tabs can be overwhelming - consider other navigation
6. **Provide custom IDs for forms**: When tabs contain form elements, use explicit IDs for better control

## Examples

Visit `/Home/Tabs` in the sample application to see:

1. **Basic Example**: Simple three-tab layout
2. **Pre-Selected Tab**: Demonstrating the `selected` attribute
3. **Custom IDs**: Using explicit tab identifiers
4. **Rich Content**: Forms, tables, and statistics in tabs

## Related Documentation

- [TAB-QUICKSTART.md](TAB-QUICKSTART.md) - Get started in 5 minutes
- [TAB-API-REFERENCE.md](TAB-API-REFERENCE.md) - Quick attribute lookup

## License

This sample usage project is provided as-is for demonstration purposes.

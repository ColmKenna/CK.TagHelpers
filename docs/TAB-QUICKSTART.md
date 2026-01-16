# Tab TagHelper - Quick Start Guide

Get started with the Tab TagHelper in 5 minutes!

## Minimal Example

This is the absolute minimum setup to get working tabbed content.

### Step 1: Register the TagHelper

In `_ViewImports.cshtml`:

```cshtml
@addTagHelper *, CK.Taghelpers
```

### Step 2: Include the CSS

In your view or `_Layout.cshtml`:

```html
<link rel="stylesheet" href="~/_content/CK.Taghelpers/css/tabs.css" />
```

### Step 3: Create Your Tabs

```cshtml
<tab>
    <tab-item heading="Tab 1">
        <p>Content for the first tab.</p>
    </tab-item>
    <tab-item heading="Tab 2">
        <p>Content for the second tab.</p>
    </tab-item>
    <tab-item heading="Tab 3">
        <p>Content for the third tab.</p>
    </tab-item>
</tab>
```

That's it! You now have a working CSS-only tabbed interface with:
- ✅ Automatic first tab selection
- ✅ Auto-generated IDs from headings
- ✅ ARIA accessibility attributes
- ✅ Responsive design

## Add More Features

### Pre-Select a Specific Tab

Use the `selected` attribute to make a specific tab active by default:

```cshtml
<tab>
    <tab-item heading="First">Content</tab-item>
    <tab-item heading="Second" selected="true">This tab is selected!</tab-item>
    <tab-item heading="Third">Content</tab-item>
</tab>
```

### Custom Tab IDs

Provide explicit IDs when you need to reference tabs programmatically:

```cshtml
<tab>
    <tab-item id="home-tab" heading="Home">Home content</tab-item>
    <tab-item id="settings-tab" heading="Settings">Settings content</tab-item>
</tab>
```

### Rich Content

Tabs can contain any HTML content - forms, tables, images, and more:

```cshtml
<tab>
    <tab-item heading="Contact Form">
        <form>
            <div class="mb-3">
                <label class="form-label">Name</label>
                <input type="text" class="form-control" />
            </div>
            <button type="submit" class="btn btn-primary">Submit</button>
        </form>
    </tab-item>
    <tab-item heading="Data Table">
        <table class="table">
            <thead>
                <tr><th>ID</th><th>Name</th></tr>
            </thead>
            <tbody>
                <tr><td>1</td><td>Example</td></tr>
            </tbody>
        </table>
    </tab-item>
</tab>
```

## Important Notes

### ⚠️ The `heading` Attribute is Required

Every `<tab-item>` must have a `heading` attribute:

```cshtml
<!-- ❌ Wrong - won't render properly -->
<tab-item>Content</tab-item>

<!-- ✅ Correct -->
<tab-item heading="My Tab">Content</tab-item>
```

### ⚠️ CSS is Required

The Tab TagHelper is CSS-only (no JavaScript needed), but you must include the CSS file:

```html
<link rel="stylesheet" href="~/_content/CK.Taghelpers/css/tabs.css" />
```

### ⚠️ Tab Items Must Be Direct Children

`<tab-item>` elements must be direct children of `<tab>`:

```cshtml
<!-- ❌ Wrong -->
<tab>
    <div>
        <tab-item heading="Tab 1">Content</tab-item>
    </div>
</tab>

<!-- ✅ Correct -->
<tab>
    <tab-item heading="Tab 1">Content</tab-item>
</tab>
```

## Common Patterns

### Organizing Settings

```cshtml
<tab>
    <tab-item heading="General">
        <h4>General Settings</h4>
        <!-- General settings form -->
    </tab-item>
    <tab-item heading="Security">
        <h4>Security Settings</h4>
        <!-- Security settings form -->
    </tab-item>
    <tab-item heading="Notifications">
        <h4>Notification Preferences</h4>
        <!-- Notification settings form -->
    </tab-item>
</tab>
```

### Content Sections

```cshtml
<tab>
    <tab-item heading="Overview">
        <h4>Product Overview</h4>
        <p>Introduction and key features...</p>
    </tab-item>
    <tab-item heading="Specifications">
        <h4>Technical Specifications</h4>
        <table class="table">...</table>
    </tab-item>
    <tab-item heading="Reviews">
        <h4>Customer Reviews</h4>
        <!-- Reviews list -->
    </tab-item>
</tab>
```

### Multiple Tab Groups

You can have multiple independent tab groups on the same page:

```cshtml
<h3>First Tab Group</h3>
<tab>
    <tab-item heading="Tab A">Content A</tab-item>
    <tab-item heading="Tab B">Content B</tab-item>
</tab>

<h3>Second Tab Group</h3>
<tab>
    <tab-item heading="Tab X">Content X</tab-item>
    <tab-item heading="Tab Y">Content Y</tab-item>
</tab>
```

Each `<tab>` container generates a unique group name, so selecting tabs in one group doesn't affect the other.

## Next Steps

- See [TAB-README.md](TAB-README.md) for complete documentation
- See [TAB-API-REFERENCE.md](TAB-API-REFERENCE.md) for attribute reference
- Run the sample project and visit `/Home/Tabs` for live examples

## Troubleshooting

**Tabs don't show**: Make sure the CSS file is included

**First tab not selected**: Check that at least one `<tab-item>` exists with a `heading` attribute

**Tab content not visible**: Verify your `<tab-item>` elements are direct children of `<tab>`

**Styling looks wrong**: Ensure no conflicting CSS is overriding the tab styles

# CK.TagHelpers

A collection of powerful ASP.NET Core TagHelpers to simplify common web development tasks.

## 📦 Installation

```bash
dotnet add package CK.TagHelpers
```

Then register the TagHelpers in your `_ViewImports.cshtml`:

```cshtml
@addTagHelper *, CK.Taghelpers
```

## 🛠️ Available TagHelpers

### Tab TagHelper

Create accessible, CSS-only tabbed interfaces with minimal markup.

```cshtml
<tab>
    <tab-item heading="Overview">Content for overview tab</tab-item>
    <tab-item heading="Details">Content for details tab</tab-item>
    <tab-item heading="Settings" selected="true">Pre-selected tab</tab-item>
</tab>
```

**Features:**
- ✅ CSS-only (no JavaScript required)
- ✅ Accessible by default (ARIA attributes)
- ✅ Auto-generated IDs from headings
- ✅ Responsive design
- ✅ First tab auto-selected if none specified

**Documentation:**
- [Quick Start Guide](docs/TAB-QUICKSTART.md) - Get started in 5 minutes
- [Complete Documentation](docs/TAB-README.md) - Full feature reference
- [API Reference](docs/TAB-API-REFERENCE.md) - Attribute and CSS reference

---

### EditArray TagHelper

Manage dynamic lists with display/edit modes, add/delete operations, and reordering.

```cshtml
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
```

**Features:**
- ✅ Display/Edit mode toggle
- ✅ Add new items dynamically
- ✅ Soft delete with undo
- ✅ Drag-and-drop reordering
- ✅ JavaScript callbacks
- ✅ Proper ASP.NET Core model binding

**Documentation:**
- [Quick Start Guide](docs/EDITARRAY-QUICKSTART.md) - Get started in 5 minutes
- [Complete Documentation](docs/EDITARRAY-README.md) - Full feature reference
- [API Reference](docs/EDITARRAY-API-REFERENCE.md) - Attribute and JS API reference
- [Troubleshooting Guide](docs/EDITARRAY-TROUBLESHOOTING.md) - Common issues and solutions

---

## 📖 Documentation

All documentation is available in the [`docs/`](docs/) folder:

| TagHelper | Quick Start | Full Docs | API Reference |
|-----------|-------------|-----------|---------------|
| **Tab** | [TAB-QUICKSTART.md](docs/TAB-QUICKSTART.md) | [TAB-README.md](docs/TAB-README.md) | [TAB-API-REFERENCE.md](docs/TAB-API-REFERENCE.md) |
| **EditArray** | [EDITARRAY-QUICKSTART.md](docs/EDITARRAY-QUICKSTART.md) | [EDITARRAY-README.md](docs/EDITARRAY-README.md) | [EDITARRAY-API-REFERENCE.md](docs/EDITARRAY-API-REFERENCE.md) |

Additional resources:
- [Documentation Index](docs/DOCUMENTATION-INDEX.md) - Navigation hub for all docs
- [Sample Project Overview](docs/SAMPLE-PROJECT-OVERVIEW.md) - Guide to the sample project

## 🚀 Sample Project

The `CK.Taghelpers.SampleUsage` project contains working examples for all TagHelpers:

```bash
cd CK.Taghelpers.SampleUsage
dotnet run
```

Then navigate to:
- `/Home/Tabs` - Tab TagHelper examples
- `/Home/Samples` - EditArray TagHelper examples
- `/Home/Documentation` - Interactive documentation guide

## 📂 Project Structure

```
CK.TagHelpers/
├── CK.Taghelpers/                 # Main TagHelper library
│   ├── TagHelpers/
│   │   ├── TabTagHelper.cs
│   │   ├── TabItemTagHelper.cs
│   │   └── EditArrayTagHelper.cs
│   └── wwwroot/
│       ├── css/tabs.css
│       └── js/editArray.js
├── CK.Taghelpers.SampleUsage/     # Sample project with examples
├── CK.TagHelpers.Tests/           # Unit tests
└── docs/                          # Documentation
    ├── TAB-*.md                   # Tab TagHelper docs
    └── EDITARRAY-*.md             # EditArray TagHelper docs
```

## 🧪 Running Tests

```bash
dotnet test
```

## 📄 License

This project is provided as-is for demonstration and production use.

# CK.TagHelpers - Documentation Index

Welcome! This is your starting point for learning about the CK.TagHelpers library.

> **📖 How to Read This Documentation**
> This file and all other `.md` files should be opened in your IDE's Markdown viewer or a Markdown editor.
> If you're viewing this from the web application, visit `/Home/Documentation` for an interactive guide to all documentation files.

## 📚 Documentation Overview

This project contains comprehensive documentation and working examples for the TagHelpers from the CK.Taghelpers library.

---

## 📁 Tab TagHelper

Create accessible, CSS-only tabbed interfaces with minimal markup.

### Quick Navigation

| Document | Best For | Time |
|----------|----------|------|
| [TAB-QUICKSTART.md](TAB-QUICKSTART.md) | Getting started quickly | 5 min |
| [TAB-README.md](TAB-README.md) | Full documentation | 15 min |
| [TAB-API-REFERENCE.md](TAB-API-REFERENCE.md) | Quick attribute lookup | Reference |
| [Views/Home/Tabs.cshtml](Views/Home/Tabs.cshtml) | Working examples | Interactive |

### If you're new to Tab TagHelper:

1. **Start here**: [TAB-QUICKSTART.md](TAB-QUICKSTART.md) (5 minutes)
2. **Run the sample**: Navigate to `/Home/Tabs` to see live examples
3. **Read the full guide**: [TAB-README.md](TAB-README.md) (15 minutes)

---

## 📋 EditArray TagHelper

Manage dynamic lists with display/edit modes, add/delete operations, and reordering.

### Quick Navigation

| Document | Best For | Time |
|----------|----------|------|
| [EDITARRAY-QUICKSTART.md](EDITARRAY-QUICKSTART.md) | Getting started quickly | 5 min |
| [EDITARRAY-README.md](EDITARRAY-README.md) | Full documentation and guide | 20 min |
| [EDITARRAY-API-REFERENCE.md](EDITARRAY-API-REFERENCE.md) | Quick attribute/function lookup | Reference |
| [EDITARRAY-TROUBLESHOOTING.md](EDITARRAY-TROUBLESHOOTING.md) | Fixing common issues | As needed |
| [Sample Project](../CK.Taghelpers.SampleUsage/Views/Home/Samples.cshtml) | Working examples | Interactive |


## 🚀 Getting Started

### If you're new to EditArrayTagHelper:

1. **Start here**: [EDITARRAY-QUICKSTART.md](EDITARRAY-QUICKSTART.md) (5 minutes)
   - Minimal working example
   - Copy-paste ready code
   - Progressive feature additions

2. **Run the sample**:
   ```bash
   dotnet run
   ```
   Navigate to `https://localhost:5001` to see 5 live examples

3. **Read the full guide**: [EDITARRAY-README.md](EDITARRAY-README.md) (20 minutes)
   - Complete feature documentation
   - Best practices
   - Advanced usage patterns

### If you're looking for something specific:

- **How do I...?** → [EDITARRAY-QUICKSTART.md](EDITARRAY-QUICKSTART.md)
- **What are all the attributes?** → [EDITARRAY-API-REFERENCE.md](EDITARRAY-API-REFERENCE.md)
- **Something's not working** → [EDITARRAY-TROUBLESHOOTING.md](EDITARRAY-TROUBLESHOOTING.md)
- **Show me working code** → Run the sample project and visit `/Home/Samples`

## 📖 Document Descriptions

### EDITARRAY-QUICKSTART.md
**Best for**: Getting up and running quickly

Contains:
- Minimal working example (just the essentials)
- Step-by-step setup instructions
- Progressive feature additions
- Common patterns
- Next steps

**Start here if**: You want to get something working ASAP

---

### EDITARRAY-README.md
**Best for**: Comprehensive understanding

Contains:
- What EditArrayTagHelper does
- Complete attribute reference
- JavaScript API documentation
- Display/edit mode patterns
- Model binding details
- Callback usage
- Styling guide
- Advanced scenarios
- Best practices

**Start here if**: You want to understand how everything works

---

### EDITARRAY-API-REFERENCE.md
**Best for**: Quick lookups while coding

Contains:
- All attributes with examples
- All JavaScript functions with signatures
- HTML structure reference
- CSS classes reference
- Data attributes reference
- Default values table
- Field name generation patterns

**Start here if**: You know what you want, just need the syntax

---

### EDITARRAY-TROUBLESHOOTING.md
**Best for**: Fixing problems

Contains:
- Common issues and solutions
- Error messages explained
- Debug checklist
- Logging techniques
- Performance tips
- Browser compatibility notes

**Start here if**: Something's not working and you need help

---

### Views/Home/Samples.cshtml
**Best for**: Learning by example

Contains 5 complete working examples:
1. Full-featured (display mode + add + reorder + callbacks)
2. Basic list (edit mode only)
3. Custom styling
4. Empty list with placeholder
5. JavaScript callbacks

**Start here if**: You learn best by seeing working code

---

## 🎯 Common Tasks

### "I want to add an editable list to my page"

1. Read: [EDITARRAY-QUICKSTART.md](EDITARRAY-QUICKSTART.md) - Minimal Example section
2. Copy the code
3. Adjust model/view names
4. Done!

### "I want to add/remove items dynamically"

1. Read: [EDITARRAY-QUICKSTART.md](EDITARRAY-QUICKSTART.md) - Enable Adding New Items section
2. Set `asp-template="true"` and `asp-add-button="true"`
3. Done!

### "I want users to reorder items"

1. Read: [EDITARRAY-QUICKSTART.md](EDITARRAY-QUICKSTART.md) - Enable Reordering section
2. Set `asp-enable-reordering="true"`
3. Done!

### "I want to do something when items are saved/deleted"

1. Read: [EDITARRAY-README.md](EDITARRAY-README.md) - Using Callbacks section
2. Set `asp-on-update` and/or `asp-on-delete`
3. Define JavaScript callback functions
4. Done!

### "I want to customize the look"

1. Read: [EDITARRAY-README.md](EDITARRAY-README.md) - Styling section
2. Use `asp-container-class`, `asp-item-class`, etc.
3. Check the sample project at `/Home/Samples` - Example 3

### "Something's broken"

1. Read: [EDITARRAY-TROUBLESHOOTING.md](EDITARRAY-TROUBLESHOOTING.md)
2. Check the Debug Checklist
3. Look for your specific error message

### "What does attribute X do?"

1. Read: [EDITARRAY-API-REFERENCE.md](EDITARRAY-API-REFERENCE.md)
2. Ctrl+F to find the attribute
3. See example usage

### "How does function X work?"

1. Read: [EDITARRAY-API-REFERENCE.md](EDITARRAY-API-REFERENCE.md) - JavaScript Functions section
2. See signature and description
3. Check working example in the sample project at `/Home/Samples`

## 🔧 Sample Project Structure

```
docs/                              ← All documentation (this folder)
├── DOCUMENTATION-INDEX.md         ← You are here!
│
├── # EditArray TagHelper Docs
├── EDITARRAY-QUICKSTART.md        ← Quick start guide
├── EDITARRAY-README.md            ← Complete documentation
├── EDITARRAY-API-REFERENCE.md     ← API reference
├── EDITARRAY-TROUBLESHOOTING.md   ← Troubleshooting guide
│
├── # Tab TagHelper Docs
├── TAB-QUICKSTART.md              ← Quick start guide
├── TAB-README.md                  ← Complete documentation
└── TAB-API-REFERENCE.md           ← API reference

CK.Taghelpers.SampleUsage/         ← Sample project with examples
├── Controllers/
├── Models/
├── Views/
│   └── Home/
│       ├── Samples.cshtml         ← EditArray examples
│       ├── Tabs.cshtml            ← Tab examples
│       └── Documentation.cshtml   ← Documentation hub
└── wwwroot/
```

## 📝 Examples in Index.cshtml

### Example 1: Full-Featured
- Display/edit mode toggle
- Add new items
- Reordering
- Delete/undelete
- Callbacks

**Use case**: Full-featured list editor with all bells and whistles

### Example 2: Basic List
- Edit mode only (no display mode)
- No add button
- No reordering

**Use case**: Simple form with multiple items

### Example 3: Custom Styling
- Custom CSS classes
- Styled container and items
- Custom button text

**Use case**: Matching your site's design

### Example 4: Empty List
- Placeholder message
- Add new items starting from empty

**Use case**: Building a list from scratch

### Example 5: Callbacks
- JavaScript callbacks on update
- JavaScript callbacks on delete
- Integration examples

**Use case**: AJAX saves, custom validation, confirmations

## 🎓 Learning Path

### Beginner
1. Read [QUICKSTART.md](QUICKSTART.md) (5 min)
2. Run the sample project and explore Example 1
3. Copy the minimal example and modify it for your needs

### Intermediate
1. Read [EDITARRAY-README.md](EDITARRAY-README.md) (20 min)
2. Explore all 5 examples in the sample project at `/Home/Samples`
3. Implement callbacks and custom styling
4. Keep [EDITARRAY-API-REFERENCE.md](EDITARRAY-API-REFERENCE.md) open while coding

### Advanced
1. Study the JavaScript in [editArray.js](wwwroot/js/editArray.js)
2. Implement AJAX saves with callbacks
3. Create custom validation logic
4. Extend functionality by modifying editArray.js

## 💡 Pro Tips

### For Best Results:

1. **Always include editArray.js** in your layout or page
2. **Always use `data-display-for`** in display partials for live updates
3. **Always set `asp-for`** for proper model binding
4. **Always include `IsDeleted`** property in your model
5. **Keep partials simple** for better performance

### Common Mistakes to Avoid:

1. ❌ Forgetting to include editArray.js
2. ❌ Missing `data-display-for` in display partial
3. ❌ Not setting `asp-for` on the edit-array tag
4. ❌ Forgetting `method="post"` on the form
5. ❌ Mismatching controller parameter names

See [EDITARRAY-TROUBLESHOOTING.md](EDITARRAY-TROUBLESHOOTING.md) for more details.

## 🔗 Related Resources

### ASP.NET Core Documentation
- [Tag Helpers](https://docs.microsoft.com/aspnet/core/mvc/views/tag-helpers/intro)
- [Model Binding](https://docs.microsoft.com/aspnet/core/mvc/models/model-binding)
- [Partial Views](https://docs.microsoft.com/aspnet/core/mvc/views/partial)

### Bootstrap (used in examples)
- [Bootstrap 5 Docs](https://getbootstrap.com/docs/5.0/)
- [Bootstrap Forms](https://getbootstrap.com/docs/5.0/forms/overview/)
- [Bootstrap Cards](https://getbootstrap.com/docs/5.0/components/card/)

### JavaScript
- [Template Element](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/template)
- [FormData API](https://developer.mozilla.org/en-US/docs/Web/API/FormData)
- [DOM Manipulation](https://developer.mozilla.org/en-US/docs/Web/API/Document_Object_Model)

## 🤝 Contributing

Found a bug or have a suggestion?

1. Check [EDITARRAY-TROUBLESHOOTING.md](EDITARRAY-TROUBLESHOOTING.md) first
2. Review the working examples in the sample project
3. Create an issue with:
   - What you expected
   - What actually happened
   - Minimal reproduction code

## 📄 License

This sample project is provided as-is for demonstration and learning purposes.

---

## Need Help?

1. **Start with**: [EDITARRAY-QUICKSTART.md](EDITARRAY-QUICKSTART.md) or [TAB-QUICKSTART.md](TAB-QUICKSTART.md)
2. **Look up syntax**: [EDITARRAY-API-REFERENCE.md](EDITARRAY-API-REFERENCE.md) or [TAB-API-REFERENCE.md](TAB-API-REFERENCE.md)
3. **Fix issues**: [EDITARRAY-TROUBLESHOOTING.md](EDITARRAY-TROUBLESHOOTING.md)
4. **Learn more**: [EDITARRAY-README.md](EDITARRAY-README.md) or [TAB-README.md](TAB-README.md)
5. **See examples**: Run the sample project with `dotnet run`

Happy coding! 🚀

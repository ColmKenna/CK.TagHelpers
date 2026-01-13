# EditArray TagHelper - Documentation Index

Welcome! This is your starting point for learning about the EditArray TagHelper.

> **📖 How to Read This Documentation**
> This file and all other `.md` files should be opened in your IDE's Markdown viewer or a Markdown editor.
> If you're viewing this from the web application, visit `/Home/Documentation` for an interactive guide to all documentation files.

## 📚 Documentation Overview

This project contains comprehensive documentation and working examples for the `EditArrayTagHelper` from the CK.Taghelpers library.

### Quick Navigation

| Document | Best For | Time |
|----------|----------|------|
| [QUICKSTART.md](QUICKSTART.md) | Getting started quickly | 5 min |
| [README.md](README.md) | Full documentation and guide | 20 min |
| [API-REFERENCE.md](API-REFERENCE.md) | Quick attribute/function lookup | Reference |
| [TROUBLESHOOTING.md](TROUBLESHOOTING.md) | Fixing common issues | As needed |
| [Views/Home/Samples.cshtml](Views/Home/Samples.cshtml) | Working examples | Interactive |

## 🚀 Getting Started

### If you're new to EditArrayTagHelper:

1. **Start here**: [QUICKSTART.md](QUICKSTART.md) (5 minutes)
   - Minimal working example
   - Copy-paste ready code
   - Progressive feature additions

2. **Run the sample**:
   ```bash
   dotnet run
   ```
   Navigate to `https://localhost:5001` to see 5 live examples

3. **Read the full guide**: [README.md](README.md) (20 minutes)
   - Complete feature documentation
   - Best practices
   - Advanced usage patterns

### If you're looking for something specific:

- **How do I...?** → [QUICKSTART.md](QUICKSTART.md)
- **What are all the attributes?** → [API-REFERENCE.md](API-REFERENCE.md)
- **Something's not working** → [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
- **Show me working code** → [Views/Home/Samples.cshtml](Views/Home/Samples.cshtml)

## 📖 Document Descriptions

### QUICKSTART.md
**Best for**: Getting up and running quickly

Contains:
- Minimal working example (just the essentials)
- Step-by-step setup instructions
- Progressive feature additions
- Common patterns
- Next steps

**Start here if**: You want to get something working ASAP

---

### README.md
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

### API-REFERENCE.md
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

### TROUBLESHOOTING.md
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

1. Read: [QUICKSTART.md](QUICKSTART.md) - Minimal Example section
2. Copy the code
3. Adjust model/view names
4. Done!

### "I want to add/remove items dynamically"

1. Read: [QUICKSTART.md](QUICKSTART.md) - Enable Adding New Items section
2. Set `asp-template="true"` and `asp-add-button="true"`
3. Done!

### "I want users to reorder items"

1. Read: [QUICKSTART.md](QUICKSTART.md) - Enable Reordering section
2. Set `asp-enable-reordering="true"`
3. Done!

### "I want to do something when items are saved/deleted"

1. Read: [README.md](README.md) - Using Callbacks section
2. Set `asp-on-update` and/or `asp-on-delete`
3. Define JavaScript callback functions
4. Done!

### "I want to customize the look"

1. Read: [README.md](README.md) - Styling section
2. Use `asp-container-class`, `asp-item-class`, etc.
3. Check [Views/Home/Samples.cshtml](Views/Home/Samples.cshtml) - Example 3

### "Something's broken"

1. Read: [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
2. Check the Debug Checklist
3. Look for your specific error message

### "What does attribute X do?"

1. Read: [API-REFERENCE.md](API-REFERENCE.md)
2. Ctrl+F to find the attribute
3. See example usage

### "How does function X work?"

1. Read: [API-REFERENCE.md](API-REFERENCE.md) - JavaScript Functions section
2. See signature and description
3. Check working example in [Views/Home/Samples.cshtml](Views/Home/Samples.cshtml)

## 🔧 Sample Project Structure

```
CK.Taghelpers.SampleUsage/
├── Controllers/
│   └── HomeController.cs          ← Sample controller with data
├── Models/
│   ├── PersonViewModel.cs         ← Sample model
│   └── TeamViewModel.cs           ← Container model
├── Views/
│   ├── Home/
│   │   └── Index.cshtml           ← 5 working examples
│   └── Shared/
│       ├── PersonEditors/
│       │   ├── _PersonEditor.cshtml    ← Edit partial
│       │   └── _PersonDisplay.cshtml   ← Display partial
│       └── _Layout.cshtml         ← Includes editArray.js
├── wwwroot/
│   └── js/
│       └── editArray.js           ← Required JavaScript
├── DOCUMENTATION-INDEX.md         ← You are here!
├── QUICKSTART.md                  ← Quick start guide
├── README.md                      ← Complete documentation
├── API-REFERENCE.md               ← API reference
└── TROUBLESHOOTING.md             ← Troubleshooting guide
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
1. Read [README.md](README.md) (20 min)
2. Explore all 5 examples in [Views/Home/Samples.cshtml](Views/Home/Samples.cshtml)
3. Implement callbacks and custom styling
4. Keep [API-REFERENCE.md](API-REFERENCE.md) open while coding

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

See [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for more details.

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

1. Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md) first
2. Review the working examples in [Views/Home/Samples.cshtml](Views/Home/Samples.cshtml)
3. Create an issue with:
   - What you expected
   - What actually happened
   - Minimal reproduction code

## 📄 License

This sample project is provided as-is for demonstration and learning purposes.

---

## Need Help?

1. **Start with**: [QUICKSTART.md](QUICKSTART.md)
2. **Look up syntax**: [API-REFERENCE.md](API-REFERENCE.md)
3. **Fix issues**: [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
4. **Learn more**: [README.md](README.md)
5. **See examples**: Run `dotnet run` and explore

Happy coding! 🚀

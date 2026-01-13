# Navigation Guide

This guide explains how to navigate the EditArray TagHelper sample project and access all documentation.

## 🗺️ Site Structure

### Web Application Pages

When you run the application (`dotnet run`), you can navigate through these pages:

1. **Home Page** (`/` or `/Home/Index`)
   - Landing page for the entire CK.TagHelpers library
   - Lists all available TagHelpers
   - Installation and setup instructions

2. **EditArray Info** (`/Home/EditArray`)
   - Overview of EditArray TagHelper
   - Features list
   - Quick start code snippet
   - Links to examples and documentation

3. **Documentation Page** (`/Home/Documentation`)
   - Guide to all documentation files
   - Explains what each document contains
   - Best for: finding the right documentation

4. **Live Examples** (`/Home/Samples`)
   - 5 working examples with full code
   - Interactive demonstrations
   - Form submission

### Navigation Flow

```
Home (/)
  └── EditArray (/Home/EditArray)
       ├── View Examples → Live Examples (/Home/Samples)
       └── View Documentation → Documentation Page (/Home/Documentation)
```

## 📚 Documentation Files

All documentation is provided as **Markdown (.md) files** in the project directory:

```
CK.Taghelpers.SampleUsage/
├── DOCUMENTATION-INDEX.md      ← Start here for navigation
├── QUICKSTART.md               ← 5-minute getting started
├── README.md                   ← Complete 20+ page guide
├── API-REFERENCE.md            ← Quick attribute/function lookup
├── TROUBLESHOOTING.md          ← Common issues & solutions
├── SAMPLE-PROJECT-OVERVIEW.md  ← About this sample project
└── NAVIGATION-GUIDE.md         ← This file
```

### How to Read Documentation Files

#### Option 1: In Your IDE (Recommended)
Most modern IDEs have built-in Markdown preview:

- **Visual Studio**: Right-click → Open With → Markdown Editor
- **VS Code**: Right-click → Open Preview (Ctrl+Shift+V)
- **Rider**: Click the preview icon in the top-right

#### Option 2: In a Markdown Viewer
- Use a dedicated Markdown viewer/editor
- Examples: Typora, MarkText, Obsidian

#### Option 3: On GitHub
- If you push this project to GitHub, the .md files will render automatically

#### Option 4: Convert to HTML
- Use a tool like pandoc to convert .md files to HTML
- Then open in your browser

### Which Documentation File to Read?

Visit the **Documentation Page** (`/Home/Documentation`) in the running application for an interactive guide, or:

| I want to... | Read this file | Time |
|--------------|----------------|------|
| Get started quickly | QUICKSTART.md | 5 min |
| Understand all features | README.md | 20 min |
| Look up an attribute | API-REFERENCE.md | Quick |
| Fix an issue | TROUBLESHOOTING.md | As needed |
| Navigate all docs | DOCUMENTATION-INDEX.md | 5 min |
| Understand this project | SAMPLE-PROJECT-OVERVIEW.md | 10 min |

## 🔗 Links in the Application

### Working Links ✅

All links in the web application use ASP.NET Core routing and work correctly:

- `asp-action="Index"` → Home page
- `asp-action="EditArray"` → EditArray info page
- `asp-action="Documentation"` → Documentation guide page
- `asp-action="Samples"` → Live examples page

### Documentation Files 📄

Documentation files (.md) are **not served by the web application**. They are meant to be:
- Opened in your IDE
- Read in a Markdown viewer
- Viewed on GitHub after pushing

The **Documentation Page** (`/Home/Documentation`) explains where each file is located and what it contains.

## 🚀 Quick Start

### For Web Browsing:

1. Run the application: `dotnet run`
2. Navigate to `http://localhost:5297` (or the port shown)
3. Click through the pages:
   - Home → EditArray → View Examples
   - Home → EditArray → View Documentation

### For Reading Documentation:

1. Open your IDE (Visual Studio, VS Code, Rider)
2. Navigate to the `CK.Taghelpers.SampleUsage` folder
3. Open any `.md` file
4. Use your IDE's Markdown preview feature

## 📖 Recommended Learning Path

### Beginner Path

1. **Run the app**: `dotnet run`
2. **Browse**: Home → EditArray → View Examples
3. **Try it**: Play with Example 1 (add, edit, delete, reorder)
4. **Read**: Open `QUICKSTART.md` in your IDE
5. **Copy**: Use the minimal example in your own project

### Intermediate Path

1. **Run the app**: Browse all pages and examples
2. **Read**: Open `README.md` for comprehensive guide
3. **Study**: Look at `Views/Home/Samples.cshtml` source code
4. **Reference**: Keep `API-REFERENCE.md` open while coding
5. **Implement**: Build your own solution

### Advanced Path

1. **Read**: All documentation files
2. **Study**: `editArray.js` source code
3. **Extend**: Modify the JavaScript for custom behavior
4. **Integrate**: Add AJAX, custom validation, etc.

## 🆘 Getting Help

### Something Not Working?

1. Check `TROUBLESHOOTING.md` for common issues
2. Review the Debug Checklist
3. Compare your code with the working examples
4. Check browser console (F12) for errors

### Need to Find Something?

1. Visit `/Home/Documentation` in the running app
2. Or open `DOCUMENTATION-INDEX.md` in your IDE
3. Or use Ctrl+F to search across .md files

### Want to See Working Code?

1. Visit `/Home/Samples` in the running app
2. View page source to see generated HTML
3. Check `Views/Home/Samples.cshtml` for Razor code
4. Review `Controllers/HomeController.cs` for data setup

## 💡 Tips

### Tip 1: Use the Documentation Page
Instead of trying to open .md files in the browser, visit `/Home/Documentation` for an interactive guide to all documentation.

### Tip 2: Open Files Side-by-Side
When learning, open the running app in your browser and the source code files in your IDE side-by-side.

### Tip 3: Use Browser DevTools
Press F12 to see the generated HTML, CSS, and JavaScript. This helps understand how the TagHelper works.

### Tip 4: Enable Markdown Preview
Configure your IDE to automatically preview Markdown files. This makes reading documentation much easier.

### Tip 5: Start with Examples
Don't try to read all documentation first. Start with the live examples, then read docs as needed.

## 🔍 Finding Things

### In the Web App
- Use the navigation menu
- All pages have "Back" links
- Use browser back button

### In Documentation
- Open `DOCUMENTATION-INDEX.md` first
- Use IDE's file search (Ctrl+P)
- Use find in files (Ctrl+Shift+F)

### In Source Code
- Check `Views/Home/` for all Razor pages
- Check `Controllers/HomeController.cs` for actions
- Check `Models/` for data structures
- Check `wwwroot/js/editArray.js` for JavaScript

## 📦 What's Where

### Views
- `Index.cshtml` - Library landing page
- `EditArray.cshtml` - EditArray overview
- `Documentation.cshtml` - Documentation guide
- `Samples.cshtml` - Live examples

### Controllers
- `HomeController.cs` - All page actions

### Documentation
- All `.md` files in project root

### JavaScript
- `wwwroot/js/editArray.js` - TagHelper client code

### Styles
- `wwwroot/css/site.css` - Custom styles

## ✅ Summary

- **Web Pages**: Navigate through the running application
- **Documentation**: Read .md files in your IDE
- **Examples**: View live demos in the browser
- **Source Code**: Study the .cshtml and .cs files

For the best experience:
1. Run the app to see it in action
2. Open .md files in your IDE to read docs
3. Compare source code with running examples
4. Use the Documentation page as your guide

Happy coding! 🚀

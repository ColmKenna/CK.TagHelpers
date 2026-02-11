using CK.Taghelpers.SampleUsage.Models.TagHelperExamples.Tab;
using Microsoft.AspNetCore.Mvc;

namespace CK.Taghelpers.SampleUsage.Controllers;

[Route("TagHelperExamples/Tab/{action=Index}")]
public class TabExamplesController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = new TabIndexViewModel
        {
            Groups = new List<TabExampleGroup>
            {
                new()
                {
                    Title = "Basic Usage",
                    Description = "Minimal markup and default rendering for CSS-only tabs.",
                    ExampleCount = 2,
                    ActionName = nameof(BasicUsage),
                    AnchorId = "minimal-tabs"
                },
                new()
                {
                    Title = "Attributes and Configuration",
                    Description = "Every attribute demonstrated: heading, id, selected, custom CSS classes, and rich content.",
                    ExampleCount = 5,
                    ActionName = nameof(AttributesAndConfiguration),
                    AnchorId = "pre-selected"
                },
                new()
                {
                    Title = "Edge Cases and Boundaries",
                    Description = "Empty headings, special characters, single tab, many tabs, and duplicate headings.",
                    ExampleCount = 4,
                    ActionName = nameof(EdgeCasesAndBoundaries),
                    AnchorId = "single-tab"
                },
                new()
                {
                    Title = "Accessibility",
                    Description = "ARIA roles, keyboard navigation, semantic HTML, and screen reader expectations.",
                    ExampleCount = 2,
                    ActionName = nameof(Accessibility),
                    AnchorId = "aria-roles"
                },
                new()
                {
                    Title = "Themes",
                    Description = "Pre-built CSS themes: dark, primary, bordered, and pill styles via class overrides.",
                    ExampleCount = 5,
                    ActionName = nameof(Themes),
                    AnchorId = "theme-dark-tabs"
                },
                new()
                {
                    Title = "Anti-Patterns",
                    Description = "Common misuses and their correct alternatives.",
                    ExampleCount = 3,
                    ActionName = nameof(AntiPatterns),
                    AnchorId = "missing-heading"
                }
            }
        };

        return View("~/Views/TagHelperExamples/Tab/Index.cshtml", model);
    }

    [HttpGet]
    public IActionResult BasicUsage()
    {
        return View("~/Views/TagHelperExamples/Tab/BasicUsage.cshtml", new TabBasicUsageViewModel());
    }

    [HttpGet]
    public IActionResult AttributesAndConfiguration()
    {
        return View("~/Views/TagHelperExamples/Tab/AttributesAndConfiguration.cshtml", new TabAttributesViewModel());
    }

    [HttpGet]
    public IActionResult EdgeCasesAndBoundaries()
    {
        return View("~/Views/TagHelperExamples/Tab/EdgeCasesAndBoundaries.cshtml", new TabEdgeCasesViewModel());
    }

    [HttpGet]
    public IActionResult Accessibility()
    {
        return View("~/Views/TagHelperExamples/Tab/Accessibility.cshtml", new TabAccessibilityViewModel());
    }

    [HttpGet]
    public IActionResult Themes()
    {
        return View("~/Views/TagHelperExamples/Tab/Themes.cshtml", new TabThemesViewModel());
    }

    [HttpGet]
    public IActionResult AntiPatterns()
    {
        return View("~/Views/TagHelperExamples/Tab/AntiPatterns.cshtml", new TabAntiPatternsViewModel());
    }
}

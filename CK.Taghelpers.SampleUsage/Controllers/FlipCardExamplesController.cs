using CK.Taghelpers.SampleUsage.Models.TagHelperExamples.FlipCard;
using Microsoft.AspNetCore.Mvc;

namespace CK.Taghelpers.SampleUsage.Controllers;

[Route("TagHelperExamples/FlipCard/{action=Index}")]
public class FlipCardExamplesController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = new FlipCardIndexViewModel
        {
            Groups = new List<FlipCardExampleGroup>
            {
                new()
                {
                    Title = "Basic Usage",
                    Description = "Minimal markup and default rendering for horizontal and vertical flip cards.",
                    ExampleCount = 3,
                    ActionName = nameof(BasicUsage),
                    AnchorId = "minimal-horizontal"
                },
                new()
                {
                    Title = "Attributes and Configuration",
                    Description = "Every attribute demonstrated: flip direction, button text, auto-height, CSS classes, and CSS custom properties.",
                    ExampleCount = 7,
                    ActionName = nameof(AttributesAndConfiguration),
                    AnchorId = "flip-direction"
                },
                new()
                {
                    Title = "Edge Cases and Boundaries",
                    Description = "Empty titles, special characters, extremely long content, and missing faces.",
                    ExampleCount = 4,
                    ActionName = nameof(EdgeCasesAndBoundaries),
                    AnchorId = "empty-title"
                },
                new()
                {
                    Title = "Accessibility",
                    Description = "ARIA attributes, keyboard navigation, focus management, and reduced motion.",
                    ExampleCount = 2,
                    ActionName = nameof(Accessibility),
                    AnchorId = "aria-attributes"
                },
                new()
                {
                    Title = "Themes",
                    Description = "Pre-built CSS custom property themes: dark, gradient, minimal, warning, and success.",
                    ExampleCount = 6,
                    ActionName = nameof(Themes),
                    AnchorId = "theme-dark"
                },
                new()
                {
                    Title = "Anti-Patterns",
                    Description = "Common misuses and their correct alternatives.",
                    ExampleCount = 3,
                    ActionName = nameof(AntiPatterns),
                    AnchorId = "missing-card-front"
                }
            }
        };

        return View("~/Views/TagHelperExamples/FlipCard/Index.cshtml", model);
    }

    [HttpGet]
    public IActionResult BasicUsage()
    {
        return View("~/Views/TagHelperExamples/FlipCard/BasicUsage.cshtml", new FlipCardBasicUsageViewModel());
    }

    [HttpGet]
    public IActionResult AttributesAndConfiguration()
    {
        return View("~/Views/TagHelperExamples/FlipCard/AttributesAndConfiguration.cshtml", new FlipCardAttributesViewModel());
    }

    [HttpGet]
    public IActionResult EdgeCasesAndBoundaries()
    {
        return View("~/Views/TagHelperExamples/FlipCard/EdgeCasesAndBoundaries.cshtml", new FlipCardEdgeCasesViewModel());
    }

    [HttpGet]
    public IActionResult Accessibility()
    {
        return View("~/Views/TagHelperExamples/FlipCard/Accessibility.cshtml", new FlipCardAccessibilityViewModel());
    }

    [HttpGet]
    public IActionResult Themes()
    {
        return View("~/Views/TagHelperExamples/FlipCard/Themes.cshtml", new FlipCardThemesViewModel());
    }

    [HttpGet]
    public IActionResult AntiPatterns()
    {
        return View("~/Views/TagHelperExamples/FlipCard/AntiPatterns.cshtml", new FlipCardAntiPatternsViewModel());
    }
}

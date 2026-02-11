using CK.Taghelpers.SampleUsage.Models.TagHelperExamples.EditArray;
using Microsoft.AspNetCore.Mvc;

namespace CK.Taghelpers.SampleUsage.Controllers;

[Route("TagHelperExamples/EditArray/{action=Index}")]
public class TagHelperExamplesController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = new EditArrayIndexViewModel
        {
            Groups = new List<EditArrayExampleGroup>
            {
                new()
                {
                    Title = "Basic Usage",
                    Description = "Required attributes and the default rendering experience.",
                    ExampleCount = 2,
                    ActionName = nameof(BasicUsage),
                    AnchorId = "minimal-required"
                },
                new()
                {
                    Title = "Attributes and Configuration",
                    Description = "Every attribute with practical combinations.",
                    ExampleCount = 6,
                    ActionName = nameof(AttributesAndConfiguration),
                    AnchorId = "template-and-add"
                },
                new()
                {
                    Title = "Model Binding and Validation",
                    Description = "Binding arrays with validation and nested models.",
                    ExampleCount = 2,
                    ActionName = nameof(ModelBindingAndValidation),
                    AnchorId = "bind-and-validate"
                },
                new()
                {
                    Title = "Form Integration",
                    Description = "Multiple arrays, partials, and ViewComponents.",
                    ExampleCount = 3,
                    ActionName = nameof(FormIntegration),
                    AnchorId = "multiple-instances"
                },
                new()
                {
                    Title = "Edge Cases and Boundaries",
                    Description = "Nulls, empty lists, long text, special characters.",
                    ExampleCount = 4,
                    ActionName = nameof(EdgeCasesAndBoundaries),
                    AnchorId = "empty-collection"
                },
                new()
                {
                    Title = "Accessibility",
                    Description = "Keyboard flow, ARIA labels, and focus expectations.",
                    ExampleCount = 1,
                    ActionName = nameof(Accessibility),
                    AnchorId = "aria-and-keyboard"
                },
                new()
                {
                    Title = "Anti-Patterns",
                    Description = "Common misuses and the safe alternatives.",
                    ExampleCount = 2,
                    ActionName = nameof(AntiPatterns),
                    AnchorId = "missing-required"
                }
            }
        };

        return View("EditArray/Index", model);
    }

    [HttpGet]
    public IActionResult BasicUsage()
    {
        var model = new EditArrayBasicUsageViewModel
        {
            MinimalMembers = EditArraySampleData.GetMinimalMembers(),
            DisplayModeMembers = EditArraySampleData.GetDisplayMembers()
        };

        return View("EditArray/BasicUsage", model);
    }

    [HttpGet]
    public IActionResult AttributesAndConfiguration()
    {
        var model = new EditArrayAttributesViewModel
        {
            TemplateMembers = EditArraySampleData.GetTemplateMembers(),
            StyledMembers = EditArraySampleData.GetStyledMembers(),
            ReorderMembers = EditArraySampleData.GetReorderMembers(),
            ButtonTextMembers = EditArraySampleData.GetButtonTextMembers(),
            CallbackMembers = EditArraySampleData.GetCallbackMembers(),
            EmptyMembers = new List<EditArrayPersonViewModel>()
        };

        return View("EditArray/AttributesAndConfiguration", model);
    }

    [HttpGet]
    public IActionResult ModelBindingAndValidation()
    {
        var model = new EditArrayValidationViewModel
        {
            RequiredMembers = EditArraySampleData.GetValidationMembers(),
            Profile = EditArraySampleData.GetProfile()
        };

        return View("EditArray/ModelBindingAndValidation", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ModelBindingAndValidation(EditArrayValidationViewModel model)
    {
        EnsureValidationDefaults(model);

        if (ModelState.IsValid)
        {
            TempData["Message"] = "Validation passed. Items are ready to save.";
        }

        return View("EditArray/ModelBindingAndValidation", model);
    }

    [HttpGet]
    public IActionResult FormIntegration()
    {
        var model = new EditArrayFormIntegrationViewModel
        {
            Engineers = EditArraySampleData.GetEngineers(),
            Designers = EditArraySampleData.GetDesigners(),
            Offices = EditArraySampleData.GetOfficeLocations(),
            PartialMembers = EditArraySampleData.GetPartialMembers(),
            ComponentMembers = EditArraySampleData.GetComponentMembers()
        };

        return View("EditArray/FormIntegration", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FormIntegration(EditArrayFormIntegrationViewModel model)
    {
        EnsureFormDefaults(model);

        if (ModelState.IsValid)
        {
            TempData["Message"] = "Form submission received for all arrays.";
        }

        return View("EditArray/FormIntegration", model);
    }

    [HttpGet]
    public IActionResult EdgeCasesAndBoundaries()
    {
        var model = new EditArrayEdgeCasesViewModel
        {
            EmptyMembers = new List<EditArrayPersonViewModel>(),
            LongNameMembers = EditArraySampleData.GetLongNameMembers(),
            SpecialCharacterMembers = EditArraySampleData.GetSpecialCharacterMembers(),
            NullValueMembers = EditArraySampleData.GetNullValueMembers()
        };

        return View("EditArray/EdgeCasesAndBoundaries", model);
    }

    [HttpGet]
    public IActionResult Accessibility()
    {
        var model = new EditArrayAccessibilityViewModel
        {
            AccessibleMembers = EditArraySampleData.GetAccessibleMembers()
        };

        return View("EditArray/Accessibility", model);
    }

    [HttpGet]
    public IActionResult AntiPatterns()
    {
        var model = new EditArrayAntiPatternsViewModel
        {
            Members = EditArraySampleData.GetAntiPatternMembers()
        };

        return View("EditArray/AntiPatterns", model);
    }

    private static void EnsureValidationDefaults(EditArrayValidationViewModel model)
    {
        model.RequiredMembers ??= new List<EditArrayPersonViewModel>();
        model.Profile ??= new EditArrayProfileViewModel();
        model.Profile.Addresses ??= new List<EditArrayAddressViewModel>();
    }

    private static void EnsureFormDefaults(EditArrayFormIntegrationViewModel model)
    {
        model.Engineers ??= new List<EditArrayPersonViewModel>();
        model.Designers ??= new List<EditArrayPersonViewModel>();
        model.Offices ??= new List<EditArrayAddressViewModel>();
        model.PartialMembers ??= new EditArrayPartialMembersViewModel();
        model.PartialMembers.Members ??= new List<EditArrayPersonViewModel>();
        model.ComponentMembers ??= new EditArrayRosterViewModel();
        model.ComponentMembers.Members ??= new List<EditArrayPersonViewModel>();
    }
}

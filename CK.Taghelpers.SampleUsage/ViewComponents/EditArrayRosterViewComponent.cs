using CK.Taghelpers.SampleUsage.Models.TagHelperExamples.EditArray;
using Microsoft.AspNetCore.Mvc;

namespace CK.Taghelpers.SampleUsage.ViewComponents;

public class EditArrayRosterViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(EditArrayRosterViewModel model)
    {
        return View(model);
    }
}

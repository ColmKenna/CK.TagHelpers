using CK.Taghelpers.SampleUsage.Models;
using Microsoft.AspNetCore.Mvc;

namespace CK.Taghelpers.SampleUsage.Controllers;

public class EditArrayController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = new TeamViewModel
        {
            Name = "Avengers",
            Members = new List<PersonViewModel>
            {
                new() { Id = 1, Name = "Tony Stark", Role = "Tech Lead" },
                new() { Id = 2, Name = "Steve Rogers", Role = "Team Lead" },
                new() { Id = 3, Name = "Natasha Romanoff", Role = "Spy" }
            }
        };
        return View(model);
    }

    [HttpPost]
    public IActionResult Index(TeamViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Filter out deleted items if you want, or handle them specifically
        // var activeMembers = model.Members.Where(m => !m.IsDeleted).ToList();

        return View("Result", model);
    }
}

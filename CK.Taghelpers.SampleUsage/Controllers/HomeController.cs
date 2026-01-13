using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CK.Taghelpers.SampleUsage.Models;

namespace CK.Taghelpers.SampleUsage.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult EditArray()
    {
        return View();
    }

    public IActionResult Documentation()
    {
        return View();
    }

    public IActionResult Samples()
    {
        var model = new TeamViewModel
        {
            Name = "Engineering Team",

            // Example 1: Display mode with template and add button
            Members = new List<PersonViewModel>
            {
                new() { Id = 1, Name = "Alice Johnson", Role = "Senior Developer" },
                new() { Id = 2, Name = "Bob Smith", Role = "Team Lead" },
                new() { Id = 3, Name = "Carol Williams", Role = "DevOps Engineer" }
            },

            // Example 2: Basic list (no display mode, no reordering)
            BasicList = new List<PersonViewModel>
            {
                new() { Id = 10, Name = "David Brown", Role = "Junior Developer" },
                new() { Id = 11, Name = "Eve Davis", Role = "QA Engineer" }
            },

            // Example 3: Custom styling
            StyledList = new List<PersonViewModel>
            {
                new() { Id = 20, Name = "Frank Miller", Role = "Architect" },
                new() { Id = 21, Name = "Grace Lee", Role = "Product Manager" }
            },

            // Example 4: Empty list
            EmptyList = new List<PersonViewModel>(),

            // Example 5: Callbacks
            CallbackList = new List<PersonViewModel>
            {
                new() { Id = 30, Name = "Henry Wilson", Role = "Backend Developer" },
                new() { Id = 31, Name = "Iris Chen", Role = "Frontend Developer" }
            }
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult Samples(TeamViewModel model)
    {
        // In a real application, you would save the data here
        // For demo purposes, we'll just return the view with the posted data

        if (ModelState.IsValid)
        {
            TempData["Message"] = "Team data saved successfully!";
        }

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

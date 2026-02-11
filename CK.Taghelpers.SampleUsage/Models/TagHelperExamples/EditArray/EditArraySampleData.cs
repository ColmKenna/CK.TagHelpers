namespace CK.Taghelpers.SampleUsage.Models.TagHelperExamples.EditArray;

public static class EditArraySampleData
{
    public static List<EditArrayPersonViewModel> GetMinimalMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 1, Name = "Avery Patel", Role = "Developer", Email = "avery.patel@example.com", YearsOfExperience = 4, Notes = "Prefers paired reviews." },
            new() { Id = 2, Name = "Jordan Lee", Role = "QA Analyst", Email = "jordan.lee@example.com", YearsOfExperience = 2, Notes = "Covers automation." }
        };
    }

    public static List<EditArrayPersonViewModel> GetDisplayMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 10, Name = "Morgan Ruiz", Role = "Team Lead", Email = "morgan.ruiz@example.com", YearsOfExperience = 8, Notes = "Focus on mentoring." },
            new() { Id = 11, Name = "Riley Chen", Role = "Product Owner", Email = "riley.chen@example.com", YearsOfExperience = 6, Notes = "Tracks roadmap updates." }
        };
    }

    public static List<EditArrayPersonViewModel> GetTemplateMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 20, Name = "Sam Quinn", Role = "Developer", Email = "sam.quinn@example.com", YearsOfExperience = 3, Notes = "Owns API layer." }
        };
    }

    public static List<EditArrayPersonViewModel> GetStyledMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 30, Name = "Taylor Finch", Role = "Designer", Email = "taylor.finch@example.com", YearsOfExperience = 5, Notes = "Prefers Figma." },
            new() { Id = 31, Name = "Casey North", Role = "Developer", Email = "casey.north@example.com", YearsOfExperience = 7, Notes = "Optimizes rendering." }
        };
    }

    public static List<EditArrayPersonViewModel> GetReorderMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 40, Name = "Drew Miles", Role = "Developer", Email = "drew.miles@example.com", YearsOfExperience = 3, Notes = "Handles integration." },
            new() { Id = 41, Name = "Skyler Ames", Role = "Developer", Email = "skyler.ames@example.com", YearsOfExperience = 1, Notes = "New starter." },
            new() { Id = 42, Name = "Parker Young", Role = "Support", Email = "parker.young@example.com", YearsOfExperience = 4, Notes = "Owns runbooks." }
        };
    }

    public static List<EditArrayPersonViewModel> GetButtonTextMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 50, Name = "Jamie Harper", Role = "Developer", Email = "jamie.harper@example.com", YearsOfExperience = 9, Notes = "Backend owner." }
        };
    }

    public static List<EditArrayPersonViewModel> GetCallbackMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 60, Name = "Robin West", Role = "Developer", Email = "robin.west@example.com", YearsOfExperience = 2, Notes = "Pairs on PRs." },
            new() { Id = 61, Name = "Quinn Baker", Role = "QA Analyst", Email = "quinn.baker@example.com", YearsOfExperience = 5, Notes = "Regression focus." }
        };
    }

    public static List<EditArrayPersonViewModel> GetValidationMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 70, Name = "Alex Monroe", Role = "Developer", Email = "alex.monroe@example.com", YearsOfExperience = 6, Notes = "Prefers short sprints." }
        };
    }

    public static EditArrayProfileViewModel GetProfile()
    {
        return new EditArrayProfileViewModel
        {
            ManagerName = "Dana Wright",
            Addresses = new List<EditArrayAddressViewModel>
            {
                new() { Line1 = "1200 Pine Street", City = "Portland", PostalCode = "97201", Country = "USA" },
                new() { Line1 = "55 King Road", City = "Dublin", PostalCode = "12345", Country = "Ireland" }
            }
        };
    }

    public static List<EditArrayAddressViewModel> GetOfficeLocations()
    {
        return new List<EditArrayAddressViewModel>
        {
            new() { Line1 = "800 Market Street", City = "San Francisco", PostalCode = "94103", Country = "USA" },
            new() { Line1 = "22 Harbor Way", City = "Seattle", PostalCode = "98101", Country = "USA" }
        };
    }

    public static List<EditArrayPersonViewModel> GetDesigners()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 80, Name = "Harper Lane", Role = "Designer", Email = "harper.lane@example.com", YearsOfExperience = 3, Notes = "Owns UI kit." }
        };
    }

    public static List<EditArrayPersonViewModel> GetEngineers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 81, Name = "Emerson Knight", Role = "Developer", Email = "emerson.knight@example.com", YearsOfExperience = 4, Notes = "Service refactors." },
            new() { Id = 82, Name = "Rowan Blake", Role = "Developer", Email = "rowan.blake@example.com", YearsOfExperience = 2, Notes = "API tests." }
        };
    }

    public static EditArrayPartialMembersViewModel GetPartialMembers()
    {
        return new EditArrayPartialMembersViewModel
        {
            Id = "partial-members",
            Title = "Support Crew",
            Members = new List<EditArrayPersonViewModel>
            {
                new() { Id = 90, Name = "Kai Bennett", Role = "Support", Email = "kai.bennett@example.com", YearsOfExperience = 5, Notes = "Tickets triage." }
            }
        };
    }

    public static EditArrayRosterViewModel GetComponentMembers()
    {
        return new EditArrayRosterViewModel
        {
            Id = "component-members",
            Members = new List<EditArrayPersonViewModel>
            {
                new() { Id = 91, Name = "Jules Grant", Role = "Developer", Email = "jules.grant@example.com", YearsOfExperience = 7, Notes = "Performance tuning." }
            }
        };
    }

    public static List<EditArrayPersonViewModel> GetLongNameMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new()
            {
                Id = 100,
                Name = "Alexandria Catherine Montgomery-Rivera the Third",
                Role = "Developer",
                Email = "alexandria.montgomery@example.com",
                YearsOfExperience = 12,
                Notes = "Long notes to test wrapping in the display template for a full-width row in the layout."
            }
        };
    }

    public static List<EditArrayPersonViewModel> GetSpecialCharacterMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new()
            {
                Id = 101,
                Name = "Terry \"TJ\" O'Neil <script>",
                Role = "Support",
                Email = "tj.oneil@example.com",
                YearsOfExperience = 1,
                Notes = "Includes HTML entities: & <> and Unicode via escapes: \u00E9 \u2603."
            }
        };
    }

    public static List<EditArrayPersonViewModel> GetNullValueMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 102, Name = null, Role = "Developer", Email = null, YearsOfExperience = 0, Notes = null }
        };
    }

    public static List<EditArrayPersonViewModel> GetAccessibleMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 110, Name = "Mason Reed", Role = "Developer", Email = "mason.reed@example.com", YearsOfExperience = 3, Notes = "Focusable controls." },
            new() { Id = 111, Name = "Ariel Stone", Role = "QA Analyst", Email = "ariel.stone@example.com", YearsOfExperience = 6, Notes = "Keyboard friendly." }
        };
    }

    public static List<EditArrayPersonViewModel> GetAntiPatternMembers()
    {
        return new List<EditArrayPersonViewModel>
        {
            new() { Id = 120, Name = "Logan Price", Role = "Developer", Email = "logan.price@example.com", YearsOfExperience = 3, Notes = "Shows bad display template." }
        };
    }
}

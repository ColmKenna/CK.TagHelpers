namespace CK.Taghelpers.SampleUsage.Models.TagHelperExamples.EditArray;

public class EditArrayFormIntegrationViewModel
{
    public List<EditArrayPersonViewModel> Engineers { get; set; } = new();
    public List<EditArrayPersonViewModel> Designers { get; set; } = new();
    public List<EditArrayAddressViewModel> Offices { get; set; } = new();
    public EditArrayPartialMembersViewModel PartialMembers { get; set; } = new();
    public EditArrayRosterViewModel ComponentMembers { get; set; } = new();
}

public class EditArrayPartialMembersViewModel
{
    public string Id { get; set; } = "partial-members";
    public string Title { get; set; } = "Partial Members";
    public List<EditArrayPersonViewModel> Members { get; set; } = new();
}

public class EditArrayRosterViewModel
{
    public string Id { get; set; } = "component-members";
    public List<EditArrayPersonViewModel> Members { get; set; } = new();
}

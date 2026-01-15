namespace CK.Taghelpers.SampleUsage.Models;

public class TeamViewModel
{
    public string Name { get; set; } = "Engineering Team";
    public List<PersonViewModel> Members { get; set; } = [];

    // Example 1: Basic List (No reordering, no display mode)
    public List<PersonViewModel> BasicList { get; set; } = [];

    // Example 3: Custom Styling
    public List<PersonViewModel> StyledList { get; set; } = [];

    // Example 4: Empty List
    public List<PersonViewModel> EmptyList { get; set; } = [];

    // Example 5: Callbacks
    public List<PersonViewModel> CallbackList { get; set; } = [];
}

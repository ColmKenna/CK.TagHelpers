namespace CK.Taghelpers.SampleUsage.Models;

public class TeamViewModel
{
    public string Name { get; set; } = "Engineering Team";
    public List<PersonViewModel> Members { get; set; } = new();

    // Example 1: Basic List (No reordering, no display mode)
    public List<PersonViewModel> BasicList { get; set; } = new();

    // Example 3: Custom Styling
    public List<PersonViewModel> StyledList { get; set; } = new();

    // Example 4: Empty List
    public List<PersonViewModel> EmptyList { get; set; } = new();

    // Example 5: Callbacks
    public List<PersonViewModel> CallbackList { get; set; } = new();
}

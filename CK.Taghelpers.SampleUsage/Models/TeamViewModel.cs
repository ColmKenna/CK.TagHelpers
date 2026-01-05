namespace CK.Taghelpers.SampleUsage.Models;

public class TeamViewModel
{
    public string Name { get; set; } = "Engineering Team";
    public List<PersonViewModel> Members { get; set; } = new();
}

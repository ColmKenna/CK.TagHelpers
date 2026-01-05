namespace CK.Taghelpers.SampleUsage.Models;

public class PersonViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}

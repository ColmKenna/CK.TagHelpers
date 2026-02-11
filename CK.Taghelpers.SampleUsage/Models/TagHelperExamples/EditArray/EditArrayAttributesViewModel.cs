namespace CK.Taghelpers.SampleUsage.Models.TagHelperExamples.EditArray;

public class EditArrayAttributesViewModel
{
    public List<EditArrayPersonViewModel> TemplateMembers { get; set; } = new();
    public List<EditArrayPersonViewModel> StyledMembers { get; set; } = new();
    public List<EditArrayPersonViewModel> ReorderMembers { get; set; } = new();
    public List<EditArrayPersonViewModel> ButtonTextMembers { get; set; } = new();
    public List<EditArrayPersonViewModel> CallbackMembers { get; set; } = new();
    public List<EditArrayPersonViewModel> EmptyMembers { get; set; } = new();
}

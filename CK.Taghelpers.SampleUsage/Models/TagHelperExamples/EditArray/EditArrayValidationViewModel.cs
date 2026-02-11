using System.ComponentModel.DataAnnotations;

namespace CK.Taghelpers.SampleUsage.Models.TagHelperExamples.EditArray;

public class EditArrayValidationViewModel
{
    public List<EditArrayPersonViewModel> RequiredMembers { get; set; } = new();
    public EditArrayProfileViewModel Profile { get; set; } = new();

    [StringLength(200)]
    public string? Comments { get; set; }
}

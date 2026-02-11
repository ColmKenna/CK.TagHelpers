using System.ComponentModel.DataAnnotations;

namespace CK.Taghelpers.SampleUsage.Models.TagHelperExamples.EditArray;

public class EditArrayProfileViewModel
{
    [Required]
    [StringLength(60)]
    public string? ManagerName { get; set; }

    public List<EditArrayAddressViewModel> Addresses { get; set; } = new();
}

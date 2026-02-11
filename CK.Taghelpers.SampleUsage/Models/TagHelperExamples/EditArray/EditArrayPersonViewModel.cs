using System.ComponentModel.DataAnnotations;

namespace CK.Taghelpers.SampleUsage.Models.TagHelperExamples.EditArray;

public class EditArrayPersonViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(60, MinimumLength = 2)]
    [NoDigits]
    public string? Name { get; set; }

    [Required]
    [StringLength(40)]
    [RegularExpression(@"^[a-zA-Z \-']+$", ErrorMessage = "Role can only contain letters, spaces, hyphens, and apostrophes.")]
    public string? Role { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Range(0, 40)]
    public int YearsOfExperience { get; set; }

    [StringLength(200)]
    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }
}

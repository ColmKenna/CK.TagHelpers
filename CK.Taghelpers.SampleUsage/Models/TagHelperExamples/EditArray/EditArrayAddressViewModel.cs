using System.ComponentModel.DataAnnotations;

namespace CK.Taghelpers.SampleUsage.Models.TagHelperExamples.EditArray;

public class EditArrayAddressViewModel
{
    [Required]
    [StringLength(80)]
    public string? Line1 { get; set; }

    [StringLength(40)]
    public string? City { get; set; }

    [Required]
    [RegularExpression("^[0-9]{5}$", ErrorMessage = "Postal code must be 5 digits.")]
    public string? PostalCode { get; set; }

    [StringLength(40)]
    public string? Country { get; set; }

    public bool IsDeleted { get; set; }
}

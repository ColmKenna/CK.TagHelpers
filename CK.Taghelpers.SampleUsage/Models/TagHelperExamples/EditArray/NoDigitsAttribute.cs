using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CK.Taghelpers.SampleUsage.Models.TagHelperExamples.EditArray;

public sealed class NoDigitsAttribute : ValidationAttribute
{
    public NoDigitsAttribute()
        : base("Value cannot contain digits.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        return !text.Any(char.IsDigit);
    }
}

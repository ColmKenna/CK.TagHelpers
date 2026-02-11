namespace CK.Taghelpers.SampleUsage.Models.TagHelperExamples.EditArray;

public class EditArrayEdgeCasesViewModel
{
    public List<EditArrayPersonViewModel> EmptyMembers { get; set; } = new();
    public List<EditArrayPersonViewModel> LongNameMembers { get; set; } = new();
    public List<EditArrayPersonViewModel> SpecialCharacterMembers { get; set; } = new();
    public List<EditArrayPersonViewModel> NullValueMembers { get; set; } = new();
}

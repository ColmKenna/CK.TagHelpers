namespace CK.Taghelpers.SampleUsage.Models.TagHelperExamples.EditArray;

public class EditArrayIndexViewModel
{
    public string TagHelperName { get; set; } = "EditArrayTagHelper";
    public List<EditArrayExampleGroup> Groups { get; set; } = new();
}

public class EditArrayExampleGroup
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ExampleCount { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string AnchorId { get; set; } = string.Empty;
}

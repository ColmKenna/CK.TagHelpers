namespace CK.Taghelpers.SampleUsage.Models.TagHelperExamples.FlipCard;

public class FlipCardIndexViewModel
{
    public string TagHelperName { get; set; } = "FlipCardTagHelper";
    public List<FlipCardExampleGroup> Groups { get; set; } = new();
}

public class FlipCardExampleGroup
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ExampleCount { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string AnchorId { get; set; } = string.Empty;
}

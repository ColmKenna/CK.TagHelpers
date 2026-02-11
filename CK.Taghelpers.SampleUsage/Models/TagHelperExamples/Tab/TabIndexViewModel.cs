namespace CK.Taghelpers.SampleUsage.Models.TagHelperExamples.Tab;

public class TabIndexViewModel
{
    public string TagHelperName { get; set; } = "TabTagHelper";
    public List<TabExampleGroup> Groups { get; set; } = new();
}

public class TabExampleGroup
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ExampleCount { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string AnchorId { get; set; } = string.Empty;
}

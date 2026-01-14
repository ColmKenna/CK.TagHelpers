using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Primitives;
using System.Text;
using System.Text.Encodings.Web;

namespace CK.Taghelpers.TagHelpers;
    
public class EditArrayTagHelper : TagHelper
{
    private const string ItemsAttributeName = "asp-items";
    private const string ViewNameAttributeName = "asp-view-name";
    private const string DisplayViewNameAttributeName = "asp-display-view-name";
    private const string ModelExpAttributeName = "asp-for";
    private const string TemplateAttributeName = "asp-template";
    private const string AddButtonAttributeName = "asp-add-button";
    private const string DisplayModeAttributeName = "asp-display-mode";
    private const string OnUpdateAttributeName = "asp-on-update";
    private const string OnDeleteAttributeName = "asp-on-delete";
    private const string ContainerCssClassAttributeName = "asp-container-class";
    private const string ItemCssClassAttributeName = "asp-item-class";
    private const string ButtonCssClassAttributeName = "asp-button-class";
    private const string EmptyPlaceholderAttributeName = "asp-empty-placeholder";
    private const string EnableReorderAttributeName = "asp-enable-reordering";
    private const string ReorderButtonCssClassAttributeName = "asp-reorder-button-class";
    private const string MoveUpButtonTextAttributeName = "asp-move-up-text";
    private const string MoveDownButtonTextAttributeName = "asp-move-down-text";
    private const string EditButtonTextAttributeName = "asp-edit-text";
    private const string DeleteButtonTextAttributeName = "asp-delete-text";
    private const string UndeleteButtonTextAttributeName = "asp-undelete-text";
    private const string DoneButtonTextAttributeName = "asp-done-text";
    private const string AddButtonTextAttributeName = "asp-add-text";
    private const string ContainerCssClassDefault = "edit-array-container";
    private const string ItemCssClassDefault = "edit-array-item";
    

    // ============================================================
    // REQUIRED PROPERTIES
    // ============================================================

    /// <summary>
    /// Gets or sets the name of the partial view used to render each item in edit mode.
    /// This property is required.
    /// </summary>
    /// <value>
    /// The path to the partial view (e.g., "~/Views/Shared/EditorTemplates/PersonEditor.cshtml" or "_PersonEditor").
    /// </value>
    /// <remarks>
    /// The view specified must exist and be accessible from the current context. The view will receive
    /// the item as its model and can use tag helpers and HTML helpers for form binding.
    /// </remarks>
    [HtmlAttributeName(ViewNameAttributeName)]
    public required string ViewName { get; set; }

    /// <summary>
    /// Gets or sets the collection of items to render in the edit array.
    /// This property is required.
    /// </summary>
    /// <value>
    /// An enumerable collection of objects to render. Use an empty collection if there are no items.
    /// Must not be <c>null</c>.
    /// </value>
    /// <remarks>
    /// Each item in the collection will be rendered using the view specified by <see cref="ViewName"/>.
    /// If <see cref="DisplayMode"/> is enabled, items are initially rendered using <see cref="DisplayViewName"/>.
    /// The collection is enumerated once during rendering.
    /// </remarks>
    [HtmlAttributeName(ItemsAttributeName)]
    public required IEnumerable<object> Items { get; set; }

    /// <summary>
    /// Gets or sets the HTML id attribute for the edit array container.
    /// This property is required.
    /// </summary>
    /// <value>
    /// A unique identifier for the container element. Must not be <c>null</c>, empty, or whitespace.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> The provided id value is automatically prefixed with "edit-array-".
    /// For example, if you set <c>id="myArray"</c>, the actual DOM element id will be <c>"edit-array-myArray"</c>.
    /// </para>
    /// <para>
    /// The id is used to generate unique identifiers for all child elements and is referenced by
    /// JavaScript functions for add, edit, delete, and reorder operations. Each item's id is derived
    /// from this container id:
    /// <list type="bullet">
    /// <item><description>Container: <c>edit-array-{id}</c></description></item>
    /// <item><description>Items container: <c>edit-array-{id}-items</c></description></item>
    /// <item><description>Individual items: <c>edit-array-{id}-item-{index}</c></description></item>
    /// <item><description>Template: <c>edit-array-{id}-template</c></description></item>
    /// <item><description>Add button: <c>edit-array-{id}-add</c></description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks. The encoded id is safe for use
    /// in both HTML attributes and JavaScript string literals.
    /// </para>
    /// <para>
    /// An <see cref="InvalidOperationException"/> is thrown during validation if this property is not set.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;!-- If you specify: --&gt;
    /// &lt;edit-array id="addresses" asp-items="Model.Addresses" ... /&gt;
    /// 
    /// &lt;!-- The rendered HTML will have: --&gt;
    /// &lt;div id="edit-array-addresses" class="edit-array-container"&gt;
    ///     &lt;div id="edit-array-addresses-items"&gt;
    ///         &lt;div id="edit-array-addresses-item-0" class="edit-array-item"&gt;...&lt;/div&gt;
    ///     &lt;/div&gt;
    /// &lt;/div&gt;
    /// </code>
    /// </example>
    [HtmlAttributeName("id")]
    public required string Id { get; set; }

    // ============================================================
    // OPTIONAL PROPERTIES (Core Functionality)
    // ============================================================

    /// <summary>
    /// Gets or sets the model expression for the collection property being edited.
    /// Optional but recommended for proper model binding.
    /// </summary>
    /// <value>
    /// A <see cref="ModelExpression"/> representing the collection property, or <c>null</c> if not specified.
    /// </value>
    /// <remarks>
    /// <para>
    /// When specified, this expression is used to generate proper field names for model binding (e.g., "Person.Addresses[0].Street").
    /// If not specified, field names will be generated based on the ViewContext's template prefix.
    /// </para>
    /// <para>
    /// This property is particularly important when the edit array is used within a larger form and needs to participate
    /// in model binding for form submissions.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(ModelExpAttributeName)]
    public ModelExpression? For { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether items should be rendered in display mode by default.
    /// </summary>
    /// <value>
    /// <c>true</c> to show items in display mode initially; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When enabled, each item is rendered with both a display view and an edit view. The display view is shown
    /// by default, and users can toggle to edit mode using Edit/Done buttons. This provides a better user experience
    /// for read-heavy scenarios.
    /// </para>
    /// <para>
    /// Requires <see cref="DisplayViewName"/> to be specified. If <see cref="DisplayViewName"/> is <c>null</c>, empty,
    /// or whitespace when this property is <c>true</c>, an <see cref="InvalidOperationException"/> will be thrown.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(DisplayModeAttributeName)]
    public bool DisplayMode { get; set; }

    /// <summary>
    /// Gets or sets the name of the partial view used to render each item in display mode.
    /// This property is required.
    /// </summary>
    /// <value>
    /// The path to the partial view for display mode. Must not be <c>null</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When <see cref="DisplayMode"/> is enabled, items are initially rendered using this view.
    /// Users can toggle between display and edit modes using Edit/Done buttons.
    /// </para>
    /// <para>
    /// This property must always be specified, even if <see cref="DisplayMode"/> is <c>false</c>.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(DisplayViewNameAttributeName)]
    public required string DisplayViewName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to render a template section for adding new items.
    /// </summary>
    /// <value>
    /// <c>true</c> to render a template section; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When enabled, an HTML <c>&lt;template&gt;</c> element is rendered containing a blueprint for new items.
    /// This template uses placeholder field names (e.g., "[__index__]") that must be replaced with actual
    /// indices when items are added via JavaScript.
    /// </para>
    /// <para>
    /// Typically used in conjunction with <see cref="ShowAddButton"/> to allow users to dynamically add new items
    /// to the collection.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(TemplateAttributeName)]
    public bool RenderTemplate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to render an "Add New Item" button.
    /// </summary>
    /// <value>
    /// <c>true</c> to show the add button; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When enabled, an "Add New Item" button is rendered that allows users to dynamically add new items
    /// to the collection. This button invokes the JavaScript <c>addNewItem()</c> function.
    /// </para>
    /// <para>
    /// Requires <see cref="RenderTemplate"/> to be <c>true</c> to function properly, as the button clones
    /// the template to create new items.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(AddButtonAttributeName)]
    public bool ShowAddButton { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable item reordering functionality.
    /// </summary>
    /// <value>
    /// <c>true</c> to enable reordering with Move Up/Down buttons; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// When enabled, each item will have "Move Up" and "Move Down" buttons that allow users to reorder items
    /// in the collection. The reorder buttons invoke the JavaScript <c>moveItem()</c> function.
    /// The container element will also have a <c>data-reorder-enabled="true"</c> attribute.
    /// </remarks>
    [HtmlAttributeName(EnableReorderAttributeName)]
    public bool EnableReordering { get; set; }

    /// <summary>
    /// Gets or sets the text to display when the collection is empty.
    /// </summary>
    /// <value>
    /// The placeholder text, or <c>null</c> to display nothing when the collection is empty.
    /// </value>
    /// <remarks>
    /// <para>
    /// When the <see cref="Items"/> collection is empty and this property is set, the specified text is
    /// rendered inside a div with the class "edit-array-placeholder". This provides user feedback that
    /// the list is empty rather than showing a blank area.
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(EmptyPlaceholderAttributeName)]
    public string? EmptyPlaceholder { get; set; }

    // ============================================================
    // CALLBACK PROPERTIES
    // ============================================================

    /// <summary>
    /// Gets or sets the JavaScript function name to invoke when a user completes editing an item (clicks "Done").
    /// The function receives the item's DOM element ID as a parameter.
    /// </summary>
    /// <value>
    /// The name of a JavaScript function to call, or <c>null</c> if no callback is needed.
    /// </value>
    /// <remarks>
    /// <para>
    /// When set, "Done" buttons will invoke both the <c>toggleEditMode</c> function and this custom callback.
    /// The callback is executed after the toggle completes. If <c>null</c> or empty, only <c>toggleEditMode</c> is called.
    /// </para>
    /// <para>
    /// The callback value is HTML-encoded to prevent XSS vulnerabilities. Only specify the function name;
    /// do not include quotes, parentheses, or other JavaScript code beyond the function identifier.
    /// </para>
    /// <para>
    /// This is particularly useful for triggering AJAX saves, form validation, or other custom logic when
    /// an item's editing is completed.
    /// </para>
    /// </remarks>
    /// <example>
    /// &lt;edit-array asp-items="Model.Items" asp-view-name="ItemEditor"
    ///              asp-display-mode="true" asp-display-view-name="ItemDisplay"
    ///              asp-on-update="saveItemChanges" /&gt;
    ///
    /// &lt;script&gt;
    ///     function saveItemChanges(itemId) {
    ///         console.log('Item updated: ' + itemId);
    ///         // Perform AJAX save, validation, or other logic
    ///     }
    /// &lt;/script&gt;
    /// </example>
    [HtmlAttributeName(OnUpdateAttributeName)]
    public string? OnUpdate { get; set; }

    /// <summary>
    /// Gets or sets the JavaScript function name to invoke after an item is marked for deletion.
    /// The function receives the item's DOM element ID as a parameter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When set, delete buttons will invoke both the markForDeletion function and this custom callback.
    /// The callback is executed after markForDeletion completes. If null or empty, only markForDeletion is called.
    /// </para>
    /// <para>
    /// The callback value is HTML-encoded to prevent XSS vulnerabilities. Only specify the function name;
    /// do not include quotes, parentheses, or other JavaScript code beyond the function identifier.
    /// </para>
    /// </remarks>
    /// <example>
    /// &lt;edit-array asp-items="Model.Items" asp-view-name="ItemEditor"
    ///              asp-display-mode="true" asp-display-view-name="ItemDisplay"
    ///              asp-on-delete="handleItemDeleted" /&gt;
    ///
    /// &lt;script&gt;
    ///     function handleItemDeleted(itemId) {
    ///         console.log('Item deleted: ' + itemId);
    ///         // Perform custom logic like AJAX calls, animations, etc.
    ///     }
    /// &lt;/script&gt;
    /// </example>
    [HtmlAttributeName(OnDeleteAttributeName)]
    public string? OnDelete { get; set; }

    // ============================================================
    // STYLING PROPERTIES
    // ============================================================

    /// <summary>
    /// Gets or sets the CSS class(es) to apply to the outer container element.
    /// </summary>
    /// <value>
    /// A string containing one or more CSS classes separated by spaces.
    /// Default is "edit-array-container".
    /// </value>
    /// <remarks>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </remarks>
    [HtmlAttributeName(ContainerCssClassAttributeName)]
    public string? ContainerCssClass { get; set; } 

    /// <summary>
    /// Gets or sets the CSS class(es) to apply to each item wrapper element.
    /// </summary>
    /// <value>
    /// A string containing one or more CSS classes separated by spaces.
    /// Default is "edit-array-item".
    /// </value>
    /// <remarks>
    /// This class is applied to the div that wraps each individual item in the collection.
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </remarks>
    [HtmlAttributeName(ItemCssClassAttributeName)]
    public string? ItemCssClass { get; set; } 

    /// <summary>
    /// Gets or sets the base CSS class(es) to apply to all buttons (Edit, Delete, Done, Add).
    /// </summary>
    /// <value>
    /// A string containing one or more CSS classes separated by spaces.
    /// Default is "btn".
    /// </value>
    /// <remarks>
    /// <para>
    /// This class is applied to all buttons generated by the tag helper, including Edit, Delete, Done, and Add buttons.
    /// Additional button-specific classes (e.g., "btn-primary", "btn-danger") are appended to this base class.
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(ButtonCssClassAttributeName)]
    public string ButtonCssClass { get; set; } = "btn";

    /// <summary>
    /// Gets or sets the CSS class(es) to apply to reorder buttons (Move Up/Down).
    /// </summary>
    /// <value>
    /// A string containing one or more CSS classes separated by spaces.
    /// Default is "btn btn-outline-secondary".
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is only used when <see cref="EnableReordering"/> is <c>true</c>.
    /// If this property is <c>null</c>, empty, or whitespace, the value of <see cref="ButtonCssClass"/> is used instead.
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(ReorderButtonCssClassAttributeName)]
    public string ReorderButtonCssClass { get; set; } = "btn btn-outline-secondary";

    /// <summary>
    /// Gets or sets the text displayed on the "Move Up" button.
    /// </summary>
    /// <value>
    /// The button text. Default is "Move Up".
    /// </value>
    /// <remarks>
    /// This property is only used when <see cref="EnableReordering"/> is <c>true</c>.
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </remarks>
    [HtmlAttributeName(MoveUpButtonTextAttributeName)]
    public string MoveUpButtonText { get; set; } = "Move Up";

    /// <summary>
    /// Gets or sets the text displayed on the "Move Down" button.
    /// </summary>
    /// <value>
    /// The button text. Default is "Move Down".
    /// </value>
    /// <remarks>
    /// This property is only used when <see cref="EnableReordering"/> is <c>true</c>.
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </remarks>
    [HtmlAttributeName(MoveDownButtonTextAttributeName)]
    public string MoveDownButtonText { get; set; } = "Move Down";

    /// <summary>
    /// Gets or sets the text displayed on the "Edit" button.
    /// </summary>
    /// <value>
    /// The button text. Default is "Edit".
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is used when <see cref="DisplayMode"/> is <c>true</c>. The Edit button appears
    /// in the display container and allows users to switch an item to edit mode.
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(EditButtonTextAttributeName)]
    public string EditButtonText { get; set; } = "Edit";

    /// <summary>
    /// Gets or sets the text displayed on the "Delete" button.
    /// </summary>
    /// <value>
    /// The button text. Default is "Delete".
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is used when <see cref="DisplayMode"/> is <c>true</c>. The Delete button appears
    /// in the display container and marks items for deletion.
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(DeleteButtonTextAttributeName)]
    public string DeleteButtonText { get; set; } = "Delete";

    /// <summary>
    /// Gets or sets the text displayed on the "Undelete" button (shown when an item is marked for deletion).
    /// </summary>
    /// <value>
    /// The button text. Default is "Undelete".
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is used when an item has been marked for deletion. Clicking the button with this text
    /// will restore the item. The value is passed to JavaScript via a data attribute on the container.
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(UndeleteButtonTextAttributeName)]
    public string UndeleteButtonText { get; set; } = "Undelete";

    /// <summary>
    /// Gets or sets the text displayed on the "Done" button.
    /// </summary>
    /// <value>
    /// The button text. Default is "Done".
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is used when <see cref="DisplayMode"/> is <c>true</c>. The Done button appears
    /// in the edit container and allows users to complete editing and return to display mode.
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(DoneButtonTextAttributeName)]
    public string DoneButtonText { get; set; } = "Done";

    /// <summary>
    /// Gets or sets the text displayed on the "Add New Item" button.
    /// </summary>
    /// <value>
    /// The button text. Default is "Add New Item".
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is used when <see cref="RenderTemplate"/> and <see cref="ShowAddButton"/> are both <c>true</c>.
    /// The Add button appears below the template section and allows users to add new items to the collection.
    /// </para>
    /// <para>
    /// The value is HTML-encoded before output to prevent XSS attacks.
    /// </para>
    /// </remarks>
    [HtmlAttributeName(AddButtonTextAttributeName)]
    public string AddButtonText { get; set; } = "Add New Item";

    // ============================================================
    // FRAMEWORK PROPERTIES
    // ============================================================

    /// <summary>
    /// Gets or sets the view context for rendering partial views.
    /// This property is automatically populated by the ASP.NET Core framework.
    /// </summary>
    /// <value>
    /// The current <see cref="Microsoft.AspNetCore.Mvc.Rendering.ViewContext"/>.
    /// Must not be <c>null</c>.
    /// </value>
    /// <remarks>
    /// This property is marked with the <see cref="ViewContextAttribute"/> and is automatically
    /// injected by the framework. It provides access to the current view's context, including
    /// ViewData, TempData, and the HttpContext. This is used internally for rendering partial views
    /// and maintaining proper model binding prefixes.
    /// </remarks>
    [ViewContext]
    [HtmlAttributeNotBound]
    public required ViewContext ViewContext { get; set; }

    private readonly IHtmlHelper _htmlHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditArrayTagHelper"/> class.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper used for rendering partial views. Must not be <c>null</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="htmlHelper"/> is <c>null</c>.</exception>
    public EditArrayTagHelper(IHtmlHelper htmlHelper)
    {
        _htmlHelper = htmlHelper ?? throw new ArgumentNullException(nameof(htmlHelper));
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Validate required configuration - returns false and renders error UI if validation fails
        if (!ValidateConfiguration(output))
        {
            return;
        }

        // Reset the TagHelper output
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        
        // Merge with existing class attribute to preserve user-specified classes
        var existingClass = output.Attributes["class"]?.Value?.ToString();
        var containerClass = GetContainerCssClass();
        var finalClass = string.IsNullOrEmpty(existingClass) 
            ? containerClass 
            : $"{existingClass} {containerClass}";
        output.Attributes.SetAttribute("class", finalClass);

        // Create an ID for the container to use with JavaScript
        string containerId = GetContainerId();
        output.Attributes.SetAttribute("id", containerId);
        if (EnableReordering)
        {
            output.Attributes.SetAttribute("data-reorder-enabled", "true");
        }

        // Add button text data attributes for JavaScript consumption
        output.Attributes.SetAttribute("data-delete-text", DeleteButtonText);
        output.Attributes.SetAttribute("data-undelete-text", UndeleteButtonText);

        // Setup HtmlHelper to be used in our views
        (_htmlHelper as IViewContextAware)?.Contextualize(ViewContext);

        // Create container for rendered items and template sections
        var sb = new StringBuilder(EstimateInitialCapacity());

        // Get the model expression prefix from ViewContext
        var modelExpressionPrefix = ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;

        // Extract the property name from the ModelExpression if provided
        string collectionName = For?.Name ?? string.Empty;

        // Render items
        await RenderItems(sb, containerId, modelExpressionPrefix, collectionName);

        // Render template section when requested
        if (RenderTemplate)
        {
            await RenderTemplateSection(sb, containerId, modelExpressionPrefix, collectionName);
        }

        // Set the output content
        output.Content.SetHtmlContent(sb.ToString());
    }

    private async Task RenderItems(StringBuilder sb, string containerId, string modelExpressionPrefix, string collectionName)
    {
        sb.Append("<div class=\"edit-array-items\" id=\"")
          .Append(containerId)
          .Append("-items\">");

        var hasItems = false;
        var index = 0;

        foreach (var item in Items)
        {
            hasItems = true;

            var fieldName = GetFieldName(modelExpressionPrefix, collectionName, index);
            var itemId = $"{containerId}-item-{index}";

            sb.Append("<div class=\"")
              .Append(GetItemCssClass())
              .Append("\" id=\"")
              .Append(itemId)
              .Append("\"");

            // Add callback data attributes for safe JS invocation (XSS prevention)
            AppendCallbackDataAttributes(sb);

            sb.Append(">");

            // Always emit the IsDeleted marker input for consistent JS contract
            sb.Append("<input type=\"hidden\" name=\"")
              .Append(fieldName)
              .Append(".IsDeleted\" value=\"false\" data-is-deleted-marker />");

            var originalPrefix = ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
            ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = fieldName;

            var viewData = new ViewDataDictionary<object>(ViewContext.ViewData)
            {
                Model = item
            };

            await RenderItemDisplayMode(sb, item, itemId, viewData);

            AppendReorderButtons(sb, containerId, itemId);

            ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = originalPrefix;

            sb.Append("</div>");

            index++;
        }

        if (!hasItems)
        {
            RenderEmptyPlaceholder(sb);
        }

        sb.Append("</div>");
    }

    private async Task RenderItemDisplayMode(StringBuilder sb, object item, string itemId, ViewDataDictionary<object> viewData)
    {
        sb.Append("<div class=\"display-container\" id=\"")
            .Append(itemId)
            .Append("-display\" ");
        if (!DisplayMode)
            sb.Append("style=\"display: none;\" ");
        sb.Append(">");

        var displayViewContent = await _htmlHelper.PartialAsync(DisplayViewName!, item, viewData);
        using (var writer = new StringWriter())
        {
            displayViewContent.WriteTo(writer, HtmlEncoder.Default);
            sb.Append(writer.ToString());
        }

        sb.Append(GenerateButton("edit", itemId, false));
        sb.Append(GenerateButton("delete", itemId, false));
        sb.Append("</div>");

        sb.Append("<div class=\"edit-container\" id=\"")
            .Append(itemId)
            .Append("-edit\" ");
        if (DisplayMode)
            sb.Append("style=\"display: none;\" ");

        sb.Append('>');

        var editorViewContent = await _htmlHelper.PartialAsync(ViewName, item, viewData);
        using (var writer = new StringWriter())
        {
            editorViewContent.WriteTo(writer, HtmlEncoder.Default);
            sb.Append(writer.ToString());
        }

        sb.Append(GenerateButton("done", itemId, false));
        sb.Append("</div>");
    }

    private async Task RenderItemEditMode(StringBuilder sb, object item, ViewDataDictionary<object> viewData)
    {
        var viewContent = await _htmlHelper.PartialAsync(ViewName, item, viewData);
        using (var writer = new StringWriter())
        {
            viewContent.WriteTo(writer, HtmlEncoder.Default);
            sb.Append(writer.ToString());
        }
    }

    private async Task RenderTemplateSection(StringBuilder sb, string containerId, string modelExpressionPrefix, string collectionName)
    {
        var templateId = $"{containerId}-template";
        sb.Append("<template id=\"")
          .Append(templateId)
          .Append("\">");

        var templateFieldName = GetFieldName(modelExpressionPrefix, collectionName, "__index__");
        var originalPrefix = ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;
        ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = templateFieldName;

        object? templateModel = null;
        var itemType = For?.ModelExplorer?.Metadata?.ElementType 
                       ?? Items.GetType().GetGenericArguments().FirstOrDefault();

        if (itemType != null)
        {
            try
            {
                templateModel = Activator.CreateInstance(itemType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create template instance of type '{itemType.Name}'. " +
                    $"Ensure the type has a parameterless constructor.", ex);
            }
        }

        var viewData = new ViewDataDictionary<object>(ViewContext.ViewData)
        {
            Model = templateModel
        };

        sb.Append("<div class=\"")
          .Append(GetItemCssClass())
          .Append("\"");

        // Add callback data attributes for safe JS invocation (XSS prevention)
        AppendCallbackDataAttributes(sb);

        sb.Append(">");

        var name = $"{templateFieldName}.IsDeleted";

        if (!string.IsNullOrWhiteSpace(DisplayViewName))
        {
            sb.Append("<div class=\"display-container\" style=\"display: none;\">");
            if (templateModel != null)
            {
                var displayViewContent = await _htmlHelper.PartialAsync(DisplayViewName!, templateModel, viewData);
                using (var writer = new StringWriter())
                {
                    displayViewContent.WriteTo(writer, HtmlEncoder.Default);
                    sb.Append(writer.ToString());
                }
            }

            sb.Append(GenerateButton("edit", null, true));
            sb.Append(GenerateButton("delete", null, true));
            sb.Append("</div>");

            sb.Append("<div class=\"edit-container\">");
        }

        if (templateModel != null)
        {
            var viewContent = await _htmlHelper.PartialAsync(ViewName, templateModel, viewData);
            using (var writer = new StringWriter())
            {
                viewContent.WriteTo(writer, HtmlEncoder.Default);
                var templateContent = writer.ToString();
                sb.Append(templateContent);
                // Always emit the IsDeleted marker input for consistent JS contract
                sb.Append("<input type=\"hidden\" name=\"")
                  .Append(name)
                  .Append("\" value=\"false\" data-is-deleted-marker />");
            }
        }

        sb.Append(GenerateButton("done", null, true));
        sb.Append("</div>");

        AppendTemplateReorderButtons(sb, containerId);

        sb.Append("</div>");
        ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = originalPrefix;
        sb.Append("</template>");

        if (ShowAddButton)
        {
            sb.Append("<button type=\"button\" class=\"")
              .Append(GetButtonCssClass())
              .Append(" btn-primary mt-2\" id=\"")
              .Append(containerId)
              .Append("-add\" aria-label=\"Add new item\" data-action=\"add\" data-container-id=\"")
              .Append(containerId)
              .Append("\" data-template-id=\"")
              .Append(templateId)
              .Append("\">")
              .Append(HtmlEncoder.Default.Encode(AddButtonText))
              .Append("</button>");
        }
    }

    private void RenderEmptyPlaceholder(StringBuilder sb)
    {
        if (string.IsNullOrWhiteSpace(EmptyPlaceholder))
        {
            return;
        }

        sb.Append("<div class=\"edit-array-placeholder\">")
          .Append(HtmlEncoder.Default.Encode(EmptyPlaceholder))
          .Append("</div>");
    }

    private void AppendReorderButtons(StringBuilder sb, string containerId, string itemId)
    {
        if (!EnableReordering)
        {
            return;
        }

        var upText = EncodeButtonText(MoveUpButtonText, "Move Up");
        var downText = EncodeButtonText(MoveDownButtonText, "Move Down");
        var encodedCssClass = GetReorderButtonCssClass();

        sb.Append("<div class=\"reorder-controls\">");
        sb.Append("<button type=\"button\" class=\"")
            .Append(encodedCssClass)
            .Append(" reorder-btn reorder-up-btn\" data-reorder-direction=\"up\" aria-label=\"Move item ")
            .Append(itemId)
            .Append(" up\" data-action=\"move\" data-container-id=\"")
            .Append(containerId)
            .Append("\" data-item-id=\"")
            .Append(itemId)
            .Append("\" data-direction=\"-1\">");
        sb.Append(upText);
        sb.Append("</button>");
        sb.Append("<button type=\"button\" class=\"")
            .Append(encodedCssClass)
            .Append(" reorder-btn reorder-down-btn\" data-reorder-direction=\"down\" aria-label=\"Move item ")
            .Append(itemId)
            .Append(" down\" data-action=\"move\" data-container-id=\"")
            .Append(containerId)
            .Append("\" data-item-id=\"")
            .Append(itemId)
            .Append("\" data-direction=\"1\">");
        sb.Append(downText);
        sb.Append("</button>");
        sb.Append("</div>");
    }

    private void AppendTemplateReorderButtons(StringBuilder sb, string containerId)
    {
        if (!EnableReordering)
        {
            return;
        }

        var upText = EncodeButtonText(MoveUpButtonText, "Move Up");
        var downText = EncodeButtonText(MoveDownButtonText, "Move Down");
        var encodedCssClass = GetReorderButtonCssClass();

        sb.Append("<div class=\"reorder-controls\">");
        sb.Append("<button type=\"button\" class=\"")
            .Append(encodedCssClass)
            .Append(" reorder-btn reorder-up-btn\" data-reorder-direction=\"up\" aria-label=\"Move item up\" data-action=\"move\" data-container-id=\"")
            .Append(containerId)
            .Append("\" data-item-id=\"closest\" data-direction=\"-1\">");
        sb.Append(upText);
        sb.Append("</button>");
        sb.Append("<button type=\"button\" class=\"")
            .Append(encodedCssClass)
            .Append(" reorder-btn reorder-down-btn\" data-reorder-direction=\"down\" aria-label=\"Move item down\" data-action=\"move\" data-container-id=\"")
            .Append(containerId)
            .Append("\" data-item-id=\"closest\" data-direction=\"1\">");
        sb.Append(downText);
        sb.Append("</button>");
        sb.Append("</div>");
    }

    /// <summary>
    /// Appends data attributes for callback functions to enable safe JavaScript invocation.
    /// </summary>
    /// <remarks>
    /// This method outputs data-on-update and data-on-delete attributes instead of inline
    /// JavaScript callbacks, preventing XSS vulnerabilities. The JavaScript code validates
    /// that callback names resolve to actual functions before invoking them.
    /// </remarks>
    /// <param name="sb">The StringBuilder to append to.</param>
    private void AppendCallbackDataAttributes(StringBuilder sb)
    {
        if (!string.IsNullOrWhiteSpace(OnUpdate))
        {
            sb.Append(" data-on-update=\"")
              .Append(HtmlEncoder.Default.Encode(OnUpdate))
              .Append("\"");
        }

        if (!string.IsNullOrWhiteSpace(OnDelete))
        {
            sb.Append(" data-on-delete=\"")
              .Append(HtmlEncoder.Default.Encode(OnDelete))
              .Append("\"");
        }
    }

    private int EstimateInitialCapacity()
    {
        const int baseCapacity = 200;
        const int perItemEstimate = 500;

        var count = 0;
        if (Items != null && Items.TryGetNonEnumeratedCount(out var knownCount))
        {
            count = knownCount;
        }
        else
        {
            count = 10; // fallback guess when count is not cheaply available
        }

        var capacity = baseCapacity + (perItemEstimate * count);
        if (RenderTemplate)
        {
            capacity += perItemEstimate; // extra budget for template markup
        }

        return capacity;
    }

    /// <summary>
    /// Validates that a CSS class string contains only safe characters.
    /// </summary>
    /// <param name="cssClass">The CSS class string to validate.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages).</param>
    /// <exception cref="InvalidOperationException">Thrown when the CSS class contains invalid characters.</exception>
    private void ValidateCssClass(string cssClass, string propertyName)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(cssClass, @"^[a-zA-Z0-9\s\-_]*$"))
        {
            throw new InvalidOperationException(
                $"{propertyName} contains invalid characters. " +
                "Only alphanumeric, spaces, hyphens, and underscores allowed.");
        }
    }

    /// <summary>
    /// Validates that an ID string contains only safe characters.
    /// </summary>
    /// <param name="id">The ID string to validate.</param>
    /// <param name="propertyName">The name of the property being validated (for error messages).</param>
    /// <exception cref="InvalidOperationException">Thrown when the ID contains invalid characters.</exception>
    private void ValidateId(string id, string propertyName)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-zA-Z0-9\-_]*$"))
        {
            throw new InvalidOperationException(
                $"{propertyName} contains invalid characters. " +
                "Only alphanumeric, hyphens, and underscores allowed.");
        }
    }

    /// <summary>
    /// Gets the reorder button CSS class with validation for safe output.
    /// </summary>
    /// <returns>The raw reorder button CSS class (falls back to ButtonCssClass if ReorderButtonCssClass is empty). Razor will encode.</returns>
    private string GetReorderButtonCssClass()
    {
        var cssClass = string.IsNullOrWhiteSpace(ReorderButtonCssClass) ? ButtonCssClass : ReorderButtonCssClass;
        ValidateCssClass(cssClass, nameof(ReorderButtonCssClass));
        return cssClass; // Return raw, Razor will encode
    }

    /// <summary>
    /// Gets the container CSS class with validation for safe output.
    /// </summary>
    /// <returns>The raw container CSS class (Razor will encode).</returns>
    private string GetContainerCssClass()
    {
        var cssClass = ContainerCssClass;
        if (string.IsNullOrWhiteSpace(cssClass))
        {
            return ContainerCssClassDefault;
        }

        ValidateCssClass(cssClass, nameof(ContainerCssClass));

        var classes = cssClass.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (!classes.Contains(ContainerCssClassDefault, StringComparer.OrdinalIgnoreCase))
        {
            cssClass = ContainerCssClassDefault + " " + cssClass;
        }

        return cssClass; // Return raw, Razor will encode
    }

    /// <summary>
    /// Gets the item CSS class with validation for safe output.
    /// </summary>
    /// <returns>The raw item CSS class (Razor will encode).</returns>
    private string GetItemCssClass()
    {
        var cssClass = ItemCssClass;
        if (string.IsNullOrWhiteSpace(cssClass))
        {
            return ItemCssClassDefault;
        }

        ValidateCssClass(cssClass, nameof(ItemCssClass));

        var classes = cssClass.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (!classes.Contains(ItemCssClassDefault, StringComparer.OrdinalIgnoreCase))
        {
            cssClass = ItemCssClassDefault + " " + cssClass;
        }

        return cssClass; // Return raw, Razor will encode
    }

    /// <summary>
    /// Gets the button CSS class with validation for safe output.
    /// </summary>
    /// <returns>The raw button CSS class (Razor will encode).</returns>
    private string GetButtonCssClass()
    {
        ValidateCssClass(ButtonCssClass, nameof(ButtonCssClass));
        return ButtonCssClass; // Return raw, Razor will encode
    }

    private string EncodeButtonText(string text, string fallback)
    {
        return HtmlEncoder.Default.Encode(string.IsNullOrWhiteSpace(text) ? fallback : text);
    }

    private string GetFieldName(string? prefix, string collectionName, object index)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            return string.IsNullOrWhiteSpace(prefix)
                ? $"[{index}]"
                : $"{prefix}[{index}]";
        }

        return string.IsNullOrWhiteSpace(prefix)
            ? $"{collectionName}[{index}]"
            : $"{prefix}.{collectionName}[{index}]";
    }

    /// <summary>
    /// Generates an HTML button element with appropriate attributes and click handlers.
    /// </summary>
    /// <param name="buttonType">The type of button: "edit", "delete", or "done"</param>
    /// <param name="itemId">The ID of the item (or null for template buttons)</param>
    /// <param name="isTemplate">True if generating button for template, false for item</param>
    /// <returns>The generated button HTML string</returns>
    private string GenerateButton(string buttonType, string? itemId, bool isTemplate = false)
    {
        var sb = new StringBuilder();

        // Determine button-specific properties
        string cssModifier, buttonText, ariaLabel;
        switch (buttonType.ToLowerInvariant())
        {
            case "edit":
                cssModifier = "btn-sm btn-primary edit-item-btn mt-2";
                buttonText = EditButtonText;
                ariaLabel = isTemplate ? "Edit item" : $"Edit item {itemId}";
                break;
            case "delete":
                cssModifier = "btn-sm btn-danger delete-item-btn mt-2";
                buttonText = DeleteButtonText;
                ariaLabel = isTemplate ? "Delete item" : $"Delete item {itemId}";
                break;
            case "done":
                cssModifier = "btn-sm btn-success done-edit-btn mt-2";
                buttonText = DoneButtonText;
                ariaLabel = isTemplate ? "Done editing item" : $"Done editing item {itemId}";
                break;
            default:
                throw new ArgumentException($"Unknown button type: {buttonType}", nameof(buttonType));
        }

        // Build the button HTML
        sb.Append("<button type=\"button\" class=\"")
          .Append(GetButtonCssClass())
          .Append(' ')
          .Append(cssModifier)
          .Append("\" aria-label=\"")
          .Append(HtmlEncoder.Default.Encode(ariaLabel))
          .Append("\"");

        // Build data attributes for event delegation (replaces inline onclick)
        sb.Append(" data-action=\"")
          .Append(buttonType.ToLowerInvariant())
          .Append("\" data-item-id=\"")
          .Append(isTemplate ? "closest" : itemId)
          .Append("\">");

        sb.Append(HtmlEncoder.Default.Encode(buttonText))
          .Append("</button>");

        return sb.ToString();
    }

    /// <summary>
    /// Gets the container ID for use in HTML attributes with validation for safe output.
    /// </summary>
    /// <remarks>
    /// The user-provided Id is validated to contain only safe characters. Razor's SetAttribute()
    /// will handle HTML encoding automatically. The resulting container ID is safe for use in both
    /// HTML attributes and JavaScript string literals within those attributes
    /// (e.g., onclick="moveItem('edit-array-id')"). All itemIds derived from this containerId inherit
    /// the same safety guarantees.
    /// </remarks>
    /// <returns>The raw container ID (e.g., "edit-array-myId"). Razor will encode.</returns>
    private string GetContainerId()
    {
        // Validate and return raw ID (Razor will encode)
        ValidateId(Id, nameof(Id));
        return $"edit-array-{Id}";
    }

    /// <summary>
    /// Validates that all required configuration properties are properly set.
    /// Instead of throwing exceptions, renders a diagnostic error panel to improve development experience.
    /// </summary>
    /// <param name="output">The TagHelperOutput to render error messages to if validation fails.</param>
    /// <returns>True if validation passed; false if validation failed and error UI was rendered.</returns>
    private bool ValidateConfiguration(TagHelperOutput output)
    {
        var errors = new List<string>();

        // Validate ViewName
        if (string.IsNullOrWhiteSpace(ViewName))
        {
            errors.Add($"'{nameof(ViewName)}' is required and must not be null or empty. Specify the name of the partial view to render for each item.");
        }

        // Validate Items
        if (Items == null)
        {
            errors.Add($"'{nameof(Items)}' is required and must not be null. Use an empty collection if there are no items to render.");
        }

        // Validate Id (required for JavaScript functionality)
        if (string.IsNullOrWhiteSpace(Id))
        {
            errors.Add($"'id' attribute is required and must not be null, empty, or whitespace. The id is used to generate unique JavaScript function calls and DOM element identifiers.");
        }

        // Validate DisplayViewName (always required as it's marked with 'required' keyword)
        if (string.IsNullOrWhiteSpace(DisplayViewName))
        {
            errors.Add($"'{nameof(DisplayViewName)}' is required and must not be null or empty. Specify the name of the partial view to render for display mode.");
        }

        // Validate ViewContext and nested properties
        if (ViewContext == null)
        {
            errors.Add($"'{nameof(ViewContext)}' is required and must not be null.");
        }
        else if (ViewContext.ViewData == null)
        {
            errors.Add("ViewContext.ViewData must not be null. Ensure ViewContext is properly initialized.");
        }

        // If there are errors, render diagnostic output instead of throwing
        if (errors.Count > 0)
        {
            RenderValidationErrors(output, errors);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Renders a diagnostic error panel showing all configuration errors.
    /// </summary>
    /// <param name="output">The TagHelperOutput to render the error panel to.</param>
    /// <param name="errors">The list of error messages to display.</param>
    private void RenderValidationErrors(TagHelperOutput output, List<string> errors)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "edit-array-error alert alert-danger");
        output.TagMode = TagMode.StartTagAndEndTag;

        var sb = new StringBuilder();
        sb.Append("<strong>EditArrayTagHelper Configuration Error:</strong>");
        sb.Append("<ul>");
        foreach (var error in errors)
        {
            sb.Append("<li>");
            sb.Append(HtmlEncoder.Default.Encode(error));
            sb.Append("</li>");
        }
        sb.Append("</ul>");

        output.Content.SetHtmlContent(sb.ToString());
    }

}
using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace CK.TagHelpers.Tests.ClientScripts;

/// <summary>
/// TDD Tests for EditArrayValidator client script - jQuery Validation integration.
///
/// Feature: Ensure editArrayValidator.js provides jQuery Validation Unobtrusive
/// support by subscribing to custom events emitted by editArray.js.
///
/// Requirements:
/// - Define an EditArrayValidator class
/// - Subscribe to editarray:init, editarray:item-added, editarray:edit-saving, editarray:edit-entered events
/// - Re-parse jQuery unobtrusive validation and attach blur handlers via wireUpValidation
/// - Validate inputs and call preventDefault on editarray:edit-saving when invalid
/// - Guard against missing jQuery
///
/// Assumptions: Script content assertions are used because there is no JS runtime test harness.
/// </summary>
public class EditArrayValidatorClientScriptTests
{
    // ========================================================================
    // 1. Class existence and structure
    // ========================================================================

    [Fact]
    public void Should_DefineEditArrayValidatorClass()
    {
        var script = LoadScript();

        Assert.Matches(
            new Regex(@"class\s+EditArrayValidator", RegexOptions.Singleline),
            script);
    }

    [Fact]
    public void Should_CreateSingletonInstance()
    {
        var script = LoadScript();

        // Should instantiate the class at file level
        Assert.Contains("new EditArrayValidator()", script);
    }

    // ========================================================================
    // 2. Event subscriptions
    // ========================================================================

    [Fact]
    public void Should_SubscribeToInitEvent()
    {
        var script = LoadScript();

        Assert.Contains("editarray:init", script);
        Assert.Contains("addEventListener", script);
    }

    [Fact]
    public void Should_SubscribeToItemAddedEvent()
    {
        var script = LoadScript();

        Assert.Contains("editarray:item-added", script);
    }

    [Fact]
    public void Should_SubscribeToEditSavingEvent()
    {
        var script = LoadScript();

        Assert.Contains("editarray:edit-saving", script);
    }

    [Fact]
    public void Should_SubscribeToEditEnteredEvent()
    {
        var script = LoadScript();

        Assert.Contains("editarray:edit-entered", script);
    }

    // ========================================================================
    // 3. Validation re-parsing (wireUpValidation logic)
    // ========================================================================

    [Fact]
    public void Should_RemoveValidatorData()
    {
        var script = LoadScript();

        Assert.Contains("removeData('validator')", script);
        Assert.Contains("removeData('unobtrusiveValidator')", script);
    }

    [Fact]
    public void Should_ParseWithUnobtrusiveValidation()
    {
        var script = LoadScript();

        Assert.Matches(
            new Regex(@"validator\.unobtrusive\.parse\s*\(", RegexOptions.Singleline),
            script);
    }

    [Fact]
    public void Should_AttachBlurValidationHandlers()
    {
        var script = LoadScript();

        Assert.Contains("blur.validate", script);
        Assert.Matches(
            new Regex(@"on\s*\(\s*['""]blur\.validate['""]", RegexOptions.Singleline),
            script);
    }

    [Fact]
    public void Should_TargetInputSelectTextareaElements()
    {
        var script = LoadScript();

        Assert.Contains("input", script);
        Assert.Contains("select", script);
        Assert.Contains("textarea", script);
    }

    [Fact]
    public void Should_GuardAgainstMissingjQuery()
    {
        var script = LoadScript();

        Assert.Contains("window.jQuery", script);
    }

    [Fact]
    public void Should_FindClosestForm()
    {
        var script = LoadScript();

        Assert.Contains("closest('form')", script);
    }

    // ========================================================================
    // 4. Input validation for edit-saving
    // ========================================================================

    [Fact]
    public void Should_CallValidOnInputs()
    {
        var script = LoadScript();

        Assert.Matches(
            new Regex(@"\.valid\s*\(\s*\)", RegexOptions.Singleline),
            script);
    }

    [Fact]
    public void Should_CallPreventDefaultWhenValidationFails()
    {
        var script = LoadScript();

        Assert.Contains("preventDefault", script);
    }

    // ========================================================================
    // Helper Methods
    // ========================================================================

    private static string LoadScript()
    {
        var baseDir = AppContext.BaseDirectory;
        var current = new DirectoryInfo(baseDir);

        while (current != null)
        {
            var primaryPath = Path.Combine(current.FullName, "CK.Taghelpers", "wwwroot", "js", "editArrayValidator.js");
            if (File.Exists(primaryPath))
            {
                return File.ReadAllText(primaryPath);
            }

            var fallbackPath = Path.Combine(current.FullName, "CK.TagHelpers", "wwwroot", "js", "editArrayValidator.js");
            if (File.Exists(fallbackPath))
            {
                return File.ReadAllText(fallbackPath);
            }

            current = current.Parent;
        }

        throw new FileNotFoundException("editArrayValidator.js not found.", baseDir);
    }
}

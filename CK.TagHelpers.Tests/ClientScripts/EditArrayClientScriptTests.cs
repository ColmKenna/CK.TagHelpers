using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace CK.TagHelpers.Tests.ClientScripts;

/// <summary>
/// TDD Tests for EditArray client script - jQuery Validation Wiring.
///
/// Feature: Ensure jQuery validation is wired up correctly for both existing
/// and new entries via a reusable wireUpValidation function.
///
/// Requirements:
/// - Extract validation re-parsing into a reusable wireUpValidation function
/// - Call wireUpValidation consistently in addNewItem and toggleEditMode
/// - Initialize validation for existing containers on page load
/// - Attach blur validation handlers for immediate field feedback
///
/// Assumptions: Script content assertions are used because there is no JS runtime test harness.
/// </summary>
public class EditArrayClientScriptTests
{
    // ========================================================================
    // 1. wireUpValidation function existence and signature
    // ========================================================================

    [Fact]
    public void Should_HaveWireUpValidationFunction()
    {
        var script = LoadScript();

        // wireUpValidation should be defined as a function taking a container parameter
        Assert.Matches(
            new Regex(@"function\s+wireUpValidation\s*\(\s*\w+\s*\)", RegexOptions.Singleline),
            script);
    }

    // ========================================================================
    // 2. wireUpValidation internals - validation re-parsing
    // ========================================================================

    [Fact]
    public void WireUpValidation_Should_RemoveValidatorData()
    {
        var script = LoadScript();

        // Extract the wireUpValidation function body
        var functionBody = ExtractFunctionBody(script, "wireUpValidation");

        // Should remove existing validator data to allow re-parsing
        Assert.Contains("removeData('validator')", functionBody);
        Assert.Contains("removeData('unobtrusiveValidator')", functionBody);
    }

    [Fact]
    public void WireUpValidation_Should_ParseWithUnobtrusiveValidation()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "wireUpValidation");

        // Should call $.validator.unobtrusive.parse
        Assert.Matches(
            new Regex(@"validator\.unobtrusive\.parse\s*\(", RegexOptions.Singleline),
            functionBody);
    }

    [Fact]
    public void WireUpValidation_Should_AttachBlurValidationHandlers()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "wireUpValidation");

        // Should attach blur validation handlers to inputs
        Assert.Contains("blur.validate", functionBody);
        Assert.Matches(
            new Regex(@"on\s*\(\s*['""]blur\.validate['""]", RegexOptions.Singleline),
            functionBody);
    }

    [Fact]
    public void WireUpValidation_Should_TargetInputSelectTextareaElements()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "wireUpValidation");

        // Should target input, select, and textarea elements for blur handlers
        Assert.Contains("input", functionBody);
        Assert.Contains("select", functionBody);
        Assert.Contains("textarea", functionBody);
    }

    [Fact]
    public void WireUpValidation_Should_GuardAgainstMissingjQuery()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "wireUpValidation");

        // Should check for jQuery and validator availability before proceeding
        Assert.Contains("window.jQuery", functionBody);
    }

    [Fact]
    public void WireUpValidation_Should_FindClosestForm()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "wireUpValidation");

        // Should find the closest form element from the container
        Assert.Contains("closest('form')", functionBody);
    }

    // ========================================================================
    // 3. addNewItem calls wireUpValidation
    // ========================================================================

    [Fact]
    public void AddNewItem_Should_CallWireUpValidation()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "addNewItem");

        // addNewItem should call wireUpValidation
        Assert.Contains("wireUpValidation", functionBody);
    }

    [Fact]
    public void AddNewItem_Should_NotContainDuplicatedValidationLogic()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "addNewItem");

        // addNewItem should NOT contain inline removeData/parse logic anymore
        // (it should delegate to wireUpValidation)
        Assert.DoesNotContain("removeData('validator')", functionBody);
        Assert.DoesNotContain("removeData('unobtrusiveValidator')", functionBody);
    }

    // ========================================================================
    // 4. toggleEditMode calls wireUpValidation
    // ========================================================================

    [Fact]
    public void ToggleEditMode_Should_CallWireUpValidation()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "toggleEditMode");

        // toggleEditMode should call wireUpValidation when entering edit mode
        Assert.Contains("wireUpValidation", functionBody);
    }

    [Fact]
    public void ToggleEditMode_Should_NotContainDuplicatedValidationLogic()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "toggleEditMode");

        // toggleEditMode should NOT contain inline removeData/parse logic anymore
        Assert.DoesNotContain("removeData('validator')", functionBody);
        Assert.DoesNotContain("removeData('unobtrusiveValidator')", functionBody);
    }

    // ========================================================================
    // 5. Page load initialization
    // ========================================================================

    [Fact]
    public void Should_InitializeValidationForExistingContainersOnPageLoad()
    {
        var script = LoadScript();

        // The script should query for existing edit-array-container elements
        // and wire up validation for them during initialization
        Assert.Contains("edit-array-container", script);

        // Should have an init function that calls wireUpValidation for existing containers
        // and is invoked on DOMContentLoaded or when readyState is already complete
        Assert.Matches(
            new Regex(
                @"querySelectorAll\s*\(\s*['""]\.edit-array-container['""]\s*\).*wireUpValidation",
                RegexOptions.Singleline),
            script);

        // The init function should be registered with DOMContentLoaded
        Assert.Matches(
            new Regex(
                @"(DOMContentLoaded|readyState).*init",
                RegexOptions.Singleline),
            script);
    }

    // ========================================================================
    // 6. toggleEditMode validates before switching to display mode
    // ========================================================================

    [Fact]
    public void ToggleEditMode_Should_ValidateEditContainerBeforeSwitchingToDisplay()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "toggleEditMode");

        // When switching from edit → display (displayContainer.style.display === 'none'),
        // should validate the edit container's form inputs before proceeding
        // Should use jQuery .valid() to trigger validation
        Assert.Matches(
            new Regex(@"\.valid\s*\(\s*\)", RegexOptions.Singleline),
            functionBody);
    }

    [Fact]
    public void ToggleEditMode_Should_ReturnEarlyWhenValidationFails()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "toggleEditMode");

        // Should check the result of validation and return early if invalid
        // The pattern should be: if validation fails → return (don't toggle)
        Assert.Matches(
            new Regex(@"valid\s*\(\s*\).*return", RegexOptions.Singleline),
            functionBody);
    }

    [Fact]
    public void ToggleEditMode_Should_CheckjQueryAvailabilityBeforeValidation()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "toggleEditMode");

        // Should guard against missing jQuery when validating
        // (vanilla JS scenario should still allow toggling without validation)
        Assert.Contains("window.jQuery", functionBody);
    }

    [Fact]
    public void ToggleEditMode_Should_FindFormForValidation()
    {
        var script = LoadScript();

        var functionBody = ExtractFunctionBody(script, "toggleEditMode");

        // Should find the closest form to validate against
        Assert.Contains("closest('form')", functionBody);
    }

    // ========================================================================
    // 7. Existing functions still present
    // ========================================================================

    [Fact]
    public void Should_StillHaveAddNewItemFunction()
    {
        var script = LoadScript();

        Assert.Matches(
            new Regex(@"function\s+addNewItem\s*\(", RegexOptions.Singleline),
            script);
    }

    [Fact]
    public void Should_StillHaveToggleEditModeFunction()
    {
        var script = LoadScript();

        Assert.Matches(
            new Regex(@"function\s+toggleEditMode\s*\(", RegexOptions.Singleline),
            script);
    }

    [Fact]
    public void Should_StillHaveHandleEditArrayActionFunction()
    {
        var script = LoadScript();

        Assert.Matches(
            new Regex(@"function\s+handleEditArrayAction\s*\(", RegexOptions.Singleline),
            script);
    }

    [Fact]
    public void Should_StillHaveEventDelegationSetup()
    {
        var script = LoadScript();

        // Event delegation should still be set up
        Assert.Contains("handleEditArrayAction", script);
        Assert.Contains("addEventListener('click'", script);
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
            var primaryPath = Path.Combine(current.FullName, "CK.Taghelpers", "wwwroot", "js", "editArray.js");
            if (File.Exists(primaryPath))
            {
                return File.ReadAllText(primaryPath);
            }

            // Also check the alternate casing
            var fallbackPath = Path.Combine(current.FullName, "CK.TagHelpers", "wwwroot", "js", "editArray.js");
            if (File.Exists(fallbackPath))
            {
                return File.ReadAllText(fallbackPath);
            }

            current = current.Parent;
        }

        throw new FileNotFoundException("editArray.js not found.", baseDir);
    }

    /// <summary>
    /// Extracts the body of a named function from the script.
    /// Finds the function declaration and returns everything between matching braces.
    /// </summary>
    private static string ExtractFunctionBody(string script, string functionName)
    {
        var pattern = $@"function\s+{Regex.Escape(functionName)}\s*\([^)]*\)\s*\{{";
        var match = Regex.Match(script, pattern);
        if (!match.Success)
        {
            throw new InvalidOperationException($"Function '{functionName}' not found in script.");
        }

        var startIndex = match.Index + match.Length;
        var braceCount = 1;
        var i = startIndex;

        while (i < script.Length && braceCount > 0)
        {
            if (script[i] == '{') braceCount++;
            else if (script[i] == '}') braceCount--;
            i++;
        }

        return script.Substring(startIndex, i - startIndex - 1);
    }
}

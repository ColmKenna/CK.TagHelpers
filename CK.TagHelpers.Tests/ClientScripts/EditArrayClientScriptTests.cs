using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace CK.TagHelpers.Tests.ClientScripts;

/// <summary>
/// TDD Tests for EditArray client script - core functionality and event dispatching.
///
/// Feature: Ensure editArray.js emits custom events at key lifecycle points
/// so that external validation modules can subscribe to them.
///
/// Requirements:
/// - Dispatch editarray:item-added after adding a new item
/// - Dispatch editarray:edit-saving (cancelable) before edit→display transition
/// - Dispatch editarray:edit-entered after display→edit transition
/// - Dispatch editarray:init for each existing container on page load
/// - Core functions (addNewItem, toggleEditMode, handleEditArrayAction) still present
///
/// Assumptions: Script content assertions are used because there is no JS runtime test harness.
/// </summary>
public class EditArrayClientScriptTests
{
    // ========================================================================
    // 1. Event dispatching - addNewItem
    // ========================================================================

    [Fact]
    public void AddNewItem_Should_DispatchItemAddedEvent()
    {
        var script = LoadScript();
        var functionBody = ExtractFunctionBody(script, "addNewItem");

        Assert.Contains("editarray:item-added", functionBody);
        Assert.Contains("dispatchEvent", functionBody);
    }

    [Fact]
    public void AddNewItem_Should_NotContainValidationLogic()
    {
        var script = LoadScript();
        var functionBody = ExtractFunctionBody(script, "addNewItem");

        // Should not contain direct validation calls - delegated to validator via events
        Assert.DoesNotContain("wireUpValidation", functionBody);
        Assert.DoesNotContain("removeData('validator')", functionBody);
        Assert.DoesNotContain("removeData('unobtrusiveValidator')", functionBody);
    }

    // ========================================================================
    // 2. Event dispatching - toggleEditMode
    // ========================================================================

    [Fact]
    public void ToggleEditMode_Should_DispatchEditSavingEvent()
    {
        var script = LoadScript();
        var functionBody = ExtractFunctionBody(script, "toggleEditMode");

        Assert.Contains("editarray:edit-saving", functionBody);
        Assert.Contains("cancelable: true", functionBody);
    }

    [Fact]
    public void ToggleEditMode_Should_CheckDefaultPreventedBeforeProceeding()
    {
        var script = LoadScript();
        var functionBody = ExtractFunctionBody(script, "toggleEditMode");

        Assert.Contains("defaultPrevented", functionBody);
    }

    [Fact]
    public void ToggleEditMode_Should_ReturnEarlyWhenEventCancelled()
    {
        var script = LoadScript();
        var functionBody = ExtractFunctionBody(script, "toggleEditMode");

        // Should check defaultPrevented and return
        Assert.Matches(
            new Regex(@"defaultPrevented.*return", RegexOptions.Singleline),
            functionBody);
    }

    [Fact]
    public void ToggleEditMode_Should_InvokeOnDoneAndAllowCancellation()
    {
        var script = LoadScript();
        var functionBody = ExtractFunctionBody(script, "toggleEditMode");

        Assert.Contains("onDone && typeof window[onDone] === 'function'", functionBody);
        Assert.Matches(
            new Regex(@"shouldContinue\s*===\s*false\s*\)\s*{\s*return", RegexOptions.Singleline),
            functionBody);
    }

    [Fact]
    public void ToggleEditMode_Should_DispatchEditEnteredEvent()
    {
        var script = LoadScript();
        var functionBody = ExtractFunctionBody(script, "toggleEditMode");

        Assert.Contains("editarray:edit-entered", functionBody);
    }

    [Fact]
    public void ToggleEditMode_Should_NotContainDirectValidationLogic()
    {
        var script = LoadScript();
        var functionBody = ExtractFunctionBody(script, "toggleEditMode");

        // Should not contain jQuery validation calls - delegated to validator via events
        Assert.DoesNotContain("wireUpValidation", functionBody);
        Assert.DoesNotContain("removeData('validator')", functionBody);
        Assert.DoesNotContain(".valid()", functionBody);
        Assert.DoesNotContain("window.jQuery", functionBody);
    }

    // ========================================================================
    // 3. Event dispatching - initEditArray
    // ========================================================================



    [Fact]
    public void InitEditArray_Should_NotContainDirectValidationLogic()
    {
        var script = LoadScript();
        var functionBody = ExtractFunctionBody(script, "initEditArray");

        Assert.DoesNotContain("wireUpValidation", functionBody);
    }

    // ========================================================================
    // 4. Core functions still present
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

        Assert.Contains("handleEditArrayAction", script);
        Assert.Contains("addEventListener('click'", script);
    }

    // ========================================================================
    // 5. No validation code remains in editArray.js
    // ========================================================================

    [Fact]
    public void Should_NotContainWireUpValidationFunction()
    {
        var script = LoadScript();

        // wireUpValidation should have been moved to the validator module
        Assert.DoesNotMatch(
            new Regex(@"function\s+wireUpValidation\s*\(", RegexOptions.Singleline),
            script);
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

using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace CK.TagHelpers.Tests.ClientScripts;

/// <summary>
/// TDD Tests for DynamicEditor client script - EventName data attribute.
///
/// Feature: Read event name from data-event-name and avoid inline JS interpolation.
///
/// New Behaviour Tests:
/// - Read event name from dialog.dataset.eventName, trim it, and default to empty string.
/// - Warn when data-event-name is missing or empty.
///
/// Edge Cases (auto-discovered):
/// - Event name contains quotes or script markup; inline JS must not interpolate it.
///
/// Requirements Source: DynamicEditorViewComponent_CodeReview.md
/// Assumptions: Script content assertions are used because there is no JS runtime test harness.
/// </summary>
public class DynamicEditorClientScriptTests
{
    [Fact]
    public void should_read_event_name_from_data_attribute_and_trim_when_initializing()
    {
        var script = LoadScript();

        Assert.Matches(
            new Regex(
                "eventName\\s*=\\s*\\(?\\s*dialog\\.dataset\\.eventName\\s*\\|\\|\\s*\"\"\\s*\\)?\\.trim\\(\\)",
                RegexOptions.Singleline),
            script);
    }

    [Fact]
    public void should_warn_when_data_event_name_is_missing_or_empty()
    {
        var script = LoadScript();

        Assert.Contains("console.warn", script);
        Assert.Contains("data-event-name", script);
    }

    #region ReplaceWithProperJsTests
    // TODO: Replace with js tests

    [Fact]
    public void should_expose_destroy_function_for_cleanup()
    {
        var script = LoadScript();

        // The DynamicEditor object should expose a destroy function
        Assert.Matches(new Regex(@"destroy\s*:", RegexOptions.Singleline), script);
    }

    [Fact]
    public void should_store_handler_references_for_removal()
    {
        var script = LoadScript();

        // Should have a registry or storage for initialized dialogs
        Assert.Matches(new Regex(@"initializedDialogs\s*=", RegexOptions.Singleline), script);
    }

    [Fact]
    public void should_remove_event_listeners_in_destroy()
    {
        var script = LoadScript();

        // Should call removeEventListener for cleanup
        Assert.Contains("removeEventListener", script);
    }

    [Fact]
    public void should_use_mutation_observer_instead_of_patching_showModal()
    {
        var script = LoadScript();

        // Should create a MutationObserver
        Assert.Contains("MutationObserver", script);
        // Should observe the dialog for attribute changes
        Assert.Contains(".observe(", script);
    }

    [Fact]
    public void should_observe_open_attribute_for_dialog_state()
    {
        var script = LoadScript();

        // Should check for 'open' attribute to detect dialog open state
        Assert.Matches(new Regex(@"['""]open['""]", RegexOptions.Singleline), script);
        Assert.Contains("attributeFilter", script);
    }

    [Fact]
    public void should_disconnect_observer_in_destroy()
    {
        var script = LoadScript();

        // Should disconnect the observer during cleanup
        Assert.Contains(".disconnect()", script);
    }

    [Fact]
    public void should_not_patch_showModal()
    {
        var script = LoadScript();

        // Should NOT override dialog.showModal
        Assert.DoesNotContain("dialog.showModal =", script);
    }

    [Fact]
    public void should_have_get_element_value_helper()
    {
        var script = LoadScript();

        // Should have a helper function for getting element values
        Assert.Matches(new Regex(@"function\s+getElementValue\s*\(", RegexOptions.Singleline), script);
    }

    [Fact]
    public void should_have_set_element_value_helper()
    {
        var script = LoadScript();

        // Should have a helper function for setting element values
        Assert.Matches(new Regex(@"function\s+setElementValue\s*\(", RegexOptions.Singleline), script);
    }

    private static string LoadScript()
    {
        var baseDir = AppContext.BaseDirectory;
        var current = new DirectoryInfo(baseDir);

        while (current != null)
        {
            var primaryPath = Path.Combine(current.FullName, "CK.TagHelpers", "wwwroot", "js", "dynamicEditor.js");
            if (File.Exists(primaryPath))
            {
                return File.ReadAllText(primaryPath);
            }

            var fallbackPath = Path.Combine(current.FullName, "wwwroot", "js", "dynamicEditor.js");
            if (File.Exists(fallbackPath))
            {
                return File.ReadAllText(fallbackPath);
            }

            current = current.Parent;
        }

        throw new FileNotFoundException("dynamicEditor.js not found.", baseDir);
    }
    #endregion
    
}

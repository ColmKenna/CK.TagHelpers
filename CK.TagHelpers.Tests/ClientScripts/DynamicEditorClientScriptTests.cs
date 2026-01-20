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
}

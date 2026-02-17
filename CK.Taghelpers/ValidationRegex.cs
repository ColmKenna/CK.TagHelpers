using System.Text.RegularExpressions;

namespace CK.Taghelpers;

/// <summary>
/// Shared compiled regex patterns used by tag helpers and view components.
/// </summary>
internal static partial class ValidationRegex
{
    /// <summary>Allows CSS class values with letters, digits, spaces, hyphens, and underscores.</summary>
    [GeneratedRegex(@"^[a-zA-Z0-9\s\-_]*$")]
    internal static partial Regex SafeCssClass();

    /// <summary>Allows HTML id values that start with a letter, followed by letters, digits, hyphens, or underscores.</summary>
    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9\-_]*$")]
    internal static partial Regex SafeId();

    /// <summary>Allows JavaScript identifier names (letters, digits, underscore, dollar sign) with a valid leading character.</summary>
    [GeneratedRegex(@"^[a-zA-Z_$][a-zA-Z0-9_$]*$")]
    internal static partial Regex SafeJsIdentifier();

    /// <summary>Allows event names with letters, digits, hyphens, and underscores.</summary>
    [GeneratedRegex(@"^[a-zA-Z0-9_-]+$")]
    internal static partial Regex SafeEventName();
}

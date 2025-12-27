namespace Nekote.Text;

/// <summary>
/// Specifies how trailing blank lines after the last visible line should be handled.
/// </summary>
public enum TrailingBlankLineHandling
{
    /// <summary>Preserve trailing blank lines.</summary>
    Preserve,

    /// <summary>Remove trailing blank lines.</summary>
    Remove
}

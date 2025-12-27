namespace Nekote.Platform;

/// <summary>
/// Represents the type of operating system.
/// </summary>
/// <remarks>
/// Currently supports Windows, Linux, and MacOS (desktop platforms).
/// Following YAGNI principles, Nekote starts with desktop platform support.
/// 
/// Future expansion to mobile platforms (Android, iOS) and browser environments (WebAssembly)
/// is planned but not yet implemented. Other platforms return <see cref="Unknown"/>.
/// </remarks>
public enum OperatingSystemType
{
    /// <summary>
    /// Windows operating system.
    /// </summary>
    Windows,

    /// <summary>
    /// Linux operating system.
    /// </summary>
    Linux,

    /// <summary>
    /// macOS (Apple desktop/laptop operating system).
    /// </summary>
    MacOS,

    /// <summary>
    /// Unknown or unsupported operating system.
    /// </summary>
    /// <remarks>
    /// This value is returned for platforms not yet supported by Nekote, including
    /// FreeBSD, Android, iOS, tvOS, watchOS, browser/WebAssembly environments, and any future platforms.
    /// Mobile and browser platform support is planned for future releases.
    /// Application logic should handle this case appropriately, typically by throwing an exception.
    /// </remarks>
    Unknown
}

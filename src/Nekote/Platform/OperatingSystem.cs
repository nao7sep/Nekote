namespace Nekote.Platform;

/// <summary>
/// Provides operating system detection and architecture information.
/// </summary>
/// <remarks>
/// <para>
/// Currently supports Windows, Linux, and macOS (desktop platforms).
/// Following YAGNI principles, Nekote starts with desktop platform support.
/// Future expansion to mobile platforms (Android, iOS) and browser environments (WebAssembly) is planned.
/// </para>
/// <para>
/// Platforms not yet supported will return <see cref="OperatingSystemType.Unknown"/> from the <see cref="Current"/> property.
/// Application logic should handle <see cref="OperatingSystemType.Unknown"/> appropriately (typically by throwing an exception).
/// </para>
/// </remarks>
public static class OperatingSystem
{
    /// <summary>
    /// Cached operating system type, determined once at startup.
    /// The OS cannot change during process lifetime.
    /// </summary>
    private static readonly OperatingSystemType _current = DetectOperatingSystem();

    /// <summary>
    /// Gets a value indicating whether the current operating system is Windows.
    /// </summary>
    public static bool IsWindows => System.OperatingSystem.IsWindows();

    /// <summary>
    /// Gets a value indicating whether the current operating system is Linux.
    /// </summary>
    public static bool IsLinux => System.OperatingSystem.IsLinux();

    /// <summary>
    /// Gets a value indicating whether the current operating system is macOS.
    /// </summary>
    public static bool IsMacOS => System.OperatingSystem.IsMacOS();

    /// <summary>
    /// Gets the current operating system as an <see cref="OperatingSystemType"/> enumeration value.
    /// </summary>
    /// <remarks>
    /// The result is cached at startup for performance, as the operating system cannot change during process lifetime.
    /// Returns <see cref="OperatingSystemType.Unknown"/> for unsupported platforms.
    /// Application logic should throw when encountering unsupported platforms rather than attempting to proceed.
    /// </remarks>
    public static OperatingSystemType Current => _current;

    /// <summary>
    /// Detects the current operating system by checking platform identifiers in priority order.
    /// </summary>
    /// <returns>The detected operating system type, or <see cref="OperatingSystemType.Unknown"/> if not recognized.</returns>
    private static OperatingSystemType DetectOperatingSystem()
    {
        if (IsWindows) return OperatingSystemType.Windows;
        if (IsLinux) return OperatingSystemType.Linux;
        if (IsMacOS) return OperatingSystemType.MacOS;
        return OperatingSystemType.Unknown;
    }
}

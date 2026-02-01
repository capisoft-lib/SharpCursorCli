using System.Runtime.InteropServices;

namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// Entry point for creating a platform-specific <see cref="ICursorAgentRunner"/>.
/// On Windows returns the Windows implementation; on Linux/OSX throws until those implementations are available.
/// </summary>
public static class CursorAgent
{
    /// <summary>
    /// Creates a runner for the current platform.
    /// Windows: returns the Windows implementation (agent path from .env CURSOR_AGENT_PATH or PATH).
    /// Linux/macOS: returns the Linux or macOS implementation (agent path from .env or PATH).
    /// </summary>
    /// <returns>Platform-specific <see cref="ICursorAgentRunner"/>.</returns>
    /// <exception cref="PlatformNotSupportedException">When running on an unsupported platform.</exception>
    public static ICursorAgentRunner CreateRunner()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new CursorAgentRunnerWindows();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new CursorAgentRunnerLinux();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new CursorAgentRunnerOSX();
        throw new PlatformNotSupportedException($"SharpCursorCli does not support the current platform ({RuntimeInformation.OSDescription}).");
    }
}

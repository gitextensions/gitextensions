namespace GitUI.Compat;

/// <summary>
///  Identifies the Flatpak sandbox boundary for external-process launch policy.
/// </summary>
public static class FlatpakEnvironment
{
    public static bool IsFlatpak()
        => OperatingSystem.IsLinux() && File.Exists("/.flatpak-info");
}

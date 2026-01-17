using System.Reflection;
using System.Runtime.InteropServices;
using GitCommands;
using GitCommands.Utils;

namespace GitUI.CommandsDialogs.SettingsDialog.ShellExtension;

public static class ModernShellExtensionManager
{
    /// <summary>
    /// The Package Family Name (PFN) derived from the AppX package manifest.
    /// This is unique to the package publisher and name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Should the subject in the signing certificate ever change, this must also be adjusted!
    /// </para>
    /// <para>
    /// To find out the publisher ID:
    /// <list type="number">
    /// <item>Sign the AppX package and try opening it.</item>
    /// <item>Once Windows prompts you to install it, continue.</item>
    /// <item>It will error out due to the <c>ExternalLocation</c> parameter missing,
    /// but the error message will contain the full PFN that needs to be set here.</item>
    /// </list>
    /// </para>
    /// </remarks>
    internal const string PackageFamilyName = "GitExtensions.ModernShellEx_wbnnev551gwxy";

    internal const string GitExtensionsShellExDllName = "GitExtensionsModernShellEx.dll";
    internal const string GitExtensionsShellExPackageName = "GitExtensionsModernShellEx.msix";

    public static bool IsSupported =>
        EnvUtils.IsWindows11OrGreater() && Environment.Is64BitOperatingSystem;

    public static bool FilesExist()
    {
        string dllPath = FindFileInBinFolders(GitExtensionsShellExDllName);
        string packagePath = FindFileInBinFolders(GitExtensionsShellExPackageName);
        return !(string.IsNullOrEmpty(dllPath) || string.IsNullOrEmpty(packagePath));
    }

    public static bool IsRegistered()
    {
        if (!IsSupported || !FilesExist())
        {
            return false;
        }

        return IsExtensionRegistered(PackageFamilyName);
    }

    public static void Register()
    {
        if (!IsSupported)
        {
            return;
        }

        string packagePath = FindFileInBinFolders(GitExtensionsShellExPackageName);
        if (string.IsNullOrEmpty(packagePath))
        {
            throw new FileNotFoundException($"Cannot find {GitExtensionsShellExPackageName}");
        }

        string externalLocation = Path.GetDirectoryName(packagePath);
        if (string.IsNullOrEmpty(externalLocation))
        {
            throw new FileNotFoundException($"Cannot find application directory");
        }

        string packageUri = new Uri(packagePath).AbsoluteUri;
        string externalLocationUri = new Uri(externalLocation).AbsoluteUri;

        int hr = RegisterExtension(packageUri, externalLocationUri);
        ThrowIfFailed(hr, "Package registration");
    }

    public static void Unregister()
    {
        if (!IsSupported || !FilesExist())
        {
            return;
        }

        int hr = UnregisterExtension(PackageFamilyName);
        ThrowIfFailed(hr, "Package unregistration");
    }

    private static string FindFileInBinFolders(string fileName)
    {
        foreach (string binDirectory in GetBinDirectories())
        {
            string filePath = Path.Combine(binDirectory, fileName);
            if (File.Exists(filePath))
            {
                return filePath;
            }
        }

        return string.Empty;

        static IEnumerable<string> GetBinDirectories()
        {
            string installDir = AppSettings.GetInstallDir();
            if (!string.IsNullOrEmpty(installDir))
            {
                yield return installDir;
            }

            string assemblyPath = Assembly.GetAssembly(typeof(ModernShellExtensionManager))?.Location;
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                string assemblyDir = Path.GetDirectoryName(assemblyPath);
                if (!string.IsNullOrEmpty(assemblyDir))
                {
                    yield return assemblyDir;
                }
            }
        }
    }

    [DllImport(GitExtensionsShellExDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    internal static extern bool IsExtensionRegistered(string packageFamilyName);

    [DllImport(GitExtensionsShellExDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    internal static extern int RegisterExtension(string packagePath, string externalLocation);

    [DllImport(GitExtensionsShellExDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    internal static extern int UnregisterExtension(string packageFamilyName);

    [DllImport(GitExtensionsShellExDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    private static extern IntPtr GetLastRegistrationError();

    internal static string GetLastRegistrationErrorMessage()
    {
        IntPtr ptr = GetLastRegistrationError();
        return ptr == IntPtr.Zero ? "" : Marshal.PtrToStringUni(ptr) ?? "";
    }

    internal static void ThrowIfFailed(int hr, string context)
    {
        if (hr >= 0)
        {
            return;
        }

        string msg = GetLastRegistrationErrorMessage();
        if (string.IsNullOrWhiteSpace(msg))
        {
            msg = context + $" failed with HRESULT 0x{hr:X8}";
        }

        throw new Exception(msg);
    }
}

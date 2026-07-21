using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GitUI.Properties;

namespace GitUI.ScriptsEngine;

// Avalonia twin of GitUI/ScriptsEngine/ScriptInfo.cs. The serialized contract is identical;
// only icon materialization changes from GDI bitmaps to Avalonia images.

// WARNING: This class is serialized to XML!
public partial class ScriptInfo
{
    [GeneratedRegex("&(?!&)", RegexOptions.ExplicitCapture)]
    private static partial Regex MnemonicAmpersandRegex { get; }

    private IImage? _icon;

    public bool Enabled { get; set; } = true;

    public string? Name { get; set; }

    public string? Command { get; set; }

    public string? Arguments { get; set; }

    public bool AddToRevisionGridContextMenu { get; set; }

    public ScriptEvent OnEvent { get; set; }

    public bool AskConfirmation { get; set; }

    public bool RunInBackground { get; set; }

    public bool IsPowerShell { get; set; }

    public int HotkeyCommandIdentifier { get; set; }

    public string? Icon
    {
        get;
        set
        {
            field = value;
            _icon = null;
        }
    }

    public string? IconFilePath
    {
        get;
        set
        {
            field = value;
            _icon = null;
        }
    }

    public string GetDisplayName() => MnemonicAmpersandRegex.Replace(Name!, "");

    public IImage? GetIcon()
    {
        if (_icon is not null)
        {
            return _icon;
        }

        if (File.Exists(IconFilePath))
        {
            try
            {
                return _icon = new Bitmap(IconFilePath);
            }
            catch
            {
                // Executables and unsupported image formats have no portable associated-icon API.
            }
        }

        if (string.IsNullOrWhiteSpace(Icon))
        {
            return null;
        }

        PropertyInfo? property = typeof(Images).GetProperty(Icon, BindingFlags.Public | BindingFlags.Static);
        if (property?.PropertyType == typeof(Bitmap))
        {
            return _icon = (Bitmap?)property.GetValue(obj: null);
        }

        Trace.WriteLine(@$"The icon ""{Icon}"" for user script ""{GetDisplayName()}"" does not exist.");
        Icon = null;
        return null;
    }
}

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GitUI.ScriptsEngine;

// WARNING: This class is serialized to XML!
public partial class ScriptInfo
{
    // Match a single '&' (lookahead to not be followed by a second '&')
    [GeneratedRegex("&(?!&)")]
    private static partial Regex MnemonicAmpersandRegex();

    private Bitmap? _icon;

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

    /// <summary>
    /// Gets or sets the icon name.
    /// </summary>
    public string? Icon
    {
        get;
        set
        {
            field = value;
            _icon = null;
        }
    }

    /// <summary>
    /// Gets or sets the path to the file containing the icon.
    /// </summary>
    public string? IconFilePath
    {
        get;
        set
        {
            field = value;
            _icon = null;
        }
    }

    /// <summary>
    ///  Returns the name with mnemonic ampersands removed.
    /// </summary>
    public string GetDisplayName() => MnemonicAmpersandRegex().Replace(Name, "");

    /// <summary>
    /// Gets the associated bitmap.
    /// </summary>
    /// <returns>Bitmap image.</returns>
    public Bitmap? GetIcon()
    {
        return _icon ??= GetIcon();

        Bitmap? GetIcon()
        {
            if (File.Exists(IconFilePath))
            {
                if (IconFilePath.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                {
                    using Icon icon = new(IconFilePath);
                    return icon.ToBitmap();
                }

                try
                {
                    using Icon? associatedIcon = System.Drawing.Icon.ExtractAssociatedIcon(IconFilePath);
                    if (associatedIcon is not null)
                    {
                        return associatedIcon.ToBitmap();
                    }
                }
                catch
                {
                }
            }

            if (string.IsNullOrWhiteSpace(Icon))
            {
                return null;
            }

            // Get all resources
            System.Resources.ResourceManager rm
                = new("GitUI.Properties.Images",
                    System.Reflection.Assembly.GetExecutingAssembly());

            Bitmap? bitmap = (Bitmap?)rm.GetObject(Icon);
            if (bitmap is null)
            {
                // Discard the invalid name in order to not search for it again, which takes long
                Trace.WriteLine(@$"The icon ""{Icon}"" for user script ""{GetDisplayName()}"" does not exist.");
                Icon = null;
            }

            return bitmap;
        }
    }
}

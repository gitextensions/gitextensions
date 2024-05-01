namespace GitUI.ScriptsEngine
{
    // WARNING: This class is serialized to XML!
    public class ScriptInfo
    {
        public ScriptInfo()
        {
            Icon = "bug";
            Enabled = true;
        }

        public bool Enabled { get; set; }

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
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets the path to the file containing the icon.
        /// </summary>
        public string? IconFilePath { get; set; }

        /// <summary>
        /// Gets the associated bitmap.
        /// </summary>
        /// <returns>Bitmap image.</returns>
        public Bitmap? GetIcon()
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

            // return icon
            return (Bitmap)rm.GetObject(Icon);
        }
    }
}

using System;
using System.Windows.Forms;

namespace GitUI.UserControls.RevisionGridClasses
{
    /// <summary>
    /// Specialized menu item that has a title and a value which will be copied to the clipboard on click.
    /// </summary>
    /// <remarks>Clipboard value is provided by a function, this allows the click event handler to react to some external changes without additional update events.</remarks>
    public class CopyToClipboardToolStripMenuItem : ToolStripMenuItem
    {
        private readonly Func<string> _value;

        public CopyToClipboardToolStripMenuItem(string text, Func<string> value, Keys shortcut = default)
            : base(text)
        {
            _value = value;
            Click += CopyToClipboard;
            ShortcutKeys = shortcut;
        }

        private void CopyToClipboard(object sender, EventArgs e)
        {
            Clipboard.SetText(_value());
        }
    }
}
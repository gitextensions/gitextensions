using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GitExtUtils;

namespace GitUI
{
    public static class ControlHotkeyExtensions
    {
        /// <summary>
        /// Properly handle Ctrl + Backspace by removing the last word before the cursor.
        /// </summary>
        /// <remarks>
        /// By default .NET TextBox inserts a strange special character instead
        /// </remarks>
        public static void EnableRemoveWordHotkey(this Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            foreach (var textBox in control.FindDescendants().OfType<TextBox>())
            {
                textBox.KeyDown += HandleKeyDown;
                textBox.Disposed += HandleDisposed;
            }
        }

        private static void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.Back))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                // emulate keyboard sequence: Ctrl + Shift + Left, Backspace
                SendKeys.Send("^+{LEFT} {BACKSPACE}");
            }
        }

        private static void HandleDisposed(object sender, EventArgs e)
        {
            ((Control)sender).Disposed -= HandleDisposed;
            ((Control)sender).KeyDown -= HandleKeyDown;
        }
    }
}
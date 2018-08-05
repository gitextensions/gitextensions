using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GitExtUtils;

namespace GitUI
{
    public static class ControlHotkeyExtensions
    {
        /// <summary>
        /// Add Ctrl + Backspace hotkey handle for the TextBox controls.
        /// This hotkey removes the last word before the cursor.
        /// </summary>
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

            foreach (var descendant in control.FindDescendants())
            {
                switch (descendant)
                {
                    case TextBox textBox:
                        // Use the secret Windows feature. See original explanation:
                        //
                        // A few people in the early days of the Internet Explorer group used the Brief editor,
                        // which uses Ctrl+Backspace as the shortcut key to delete the previous word,
                        // and they liked it so much that one of them added it to the autocomplete handler.
                        // Therefore, any edit control that uses SHAutoComplete will gain this secret Ctrl+Backspace hotkey.
                        //
                        // More detail here: https://blogs.msdn.microsoft.com/oldnewthing/20071011-00/?p=24823/
                        const uint SHACF_AUTOSUGGEST_FORCE_OFF = 0x20000000;
                        NativeMethods.SHAutoComplete(textBox.Handle, SHACF_AUTOSUGGEST_FORCE_OFF);

                        break;
                }
            }
        }

        private static class NativeMethods
        {
            [DllImport("shlwapi.dll", ExactSpelling = true, PreserveSig = false)]
            public static extern int SHAutoComplete(IntPtr hwndEdit, uint flags);
        }
    }
}
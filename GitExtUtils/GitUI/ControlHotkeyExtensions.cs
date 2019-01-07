using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

                // Call begin / end update to prevent flickering transient state,
                // after the word is selected and before it is erased.
                _beginUpdateMethod.Invoke(sender, parameters: null);

                // emulate keyboard sequence: Ctrl + Shift + Left, Backspace
                SendKeys.Send("^+{LEFT} {BACKSPACE}");

                _endUpdateMethod.Invoke(sender, parameters: null);
            }
        }

        private static void HandleDisposed(object sender, EventArgs e)
        {
            ((Control)sender).Disposed -= HandleDisposed;
            ((Control)sender).KeyDown -= HandleKeyDown;
        }

        private static readonly MethodInfo _beginUpdateMethod =
            typeof(Control).GetMethod("BeginUpdateInternal",
                BindingFlags.Instance | BindingFlags.NonPublic);

        // there are 2 EndUpdateInternal methods, we are looking the one with parameterless signature
        private static readonly MethodInfo _endUpdateMethod =
            typeof(Control).GetMethod("EndUpdateInternal",
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                types: new Type[0],
                modifiers: new ParameterModifier[0]);
    }
}
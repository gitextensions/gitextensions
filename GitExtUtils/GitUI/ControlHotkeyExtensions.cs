using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

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

            foreach (var textInput in control.FindDescendants().Where(c => c is TextBoxBase || c is ComboBox))
            {
                textInput.KeyDown += HandleKeyDown;
                textInput.Disposed += HandleDisposed;
            }
        }

        private static void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.Back))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                int selectionStart = GetSelectionStart();
                int selectionLength = GetSelectionLength();
                string text = GetText();

                if (selectionLength > 0)
                {
                    RemoveTextRange(selectionStart, selectionLength);
                }
                else
                {
                    int previousWordBreak = FindPreviousWordStart(text, selectionStart);
                    if (previousWordBreak >= 0)
                    {
                        RemoveTextRange(previousWordBreak, selectionStart - previousWordBreak);
                    }
                }

                void RemoveTextRange(int from, int length)
                {
                    // Call begin / end update to prevent flickering transient state,
                    // after the word is selected and before it is erased.
                    _beginUpdateMethod.Invoke(sender, parameters: null);

                    SetText(text.Substring(0, from) + text.Substring(from + length));

                    SetSelectionStart(from);
                    SetSelectionLength(0);

                    _endUpdateMethod.Invoke(sender, parameters: null);

                    (sender as ComboBox)?.Refresh();
                }

                int FindPreviousWordStart(string value, int position)
                {
                    for (int i = position - 1; i >= 0; i--)
                    {
                        if (i == 0 || (!char.IsLetterOrDigit(value[i - 1]) && char.IsLetterOrDigit(value[i])))
                        {
                            return i;
                        }
                    }

                    return -1;
                }

                string GetText() =>
                    ((Control)sender).Text;

                void SetText(string value) =>
                    ((Control)sender).Text = value;

                int GetSelectionStart()
                {
                    switch (sender)
                    {
                        case TextBoxBase t:
                            return t.SelectionStart;
                        case ComboBox cb:
                            return cb.SelectionStart;
                        default:
                            throw new NotSupportedException();
                    }
                }

                void SetSelectionStart(int value)
                {
                    switch (sender)
                    {
                        case TextBoxBase t:
                            t.SelectionStart = value;
                            return;
                        case ComboBox cb:
                            cb.SelectionStart = value;
                            return;
                        default:
                            throw new NotSupportedException();
                    }
                }

                int GetSelectionLength()
                {
                    switch (sender)
                    {
                        case TextBoxBase t:
                            return t.SelectionLength;
                        case ComboBox cb:
                            return cb.SelectionLength;
                        default:
                            throw new NotSupportedException();
                    }
                }

                void SetSelectionLength(int value)
                {
                    switch (sender)
                    {
                        case TextBoxBase t:
                            t.SelectionLength = value;
                            return;
                        case ComboBox cb:
                            cb.SelectionLength = value;
                            return;
                        default:
                            throw new NotSupportedException();
                    }
                }
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
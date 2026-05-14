using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GitUI;

/// <summary>
/// Makes Ctrl+Left/Right, Ctrl+Shift+Left/Right and double-click word selection treat
/// punctuation such as '/', ':', '.', '(' and ')' as word boundaries in plain text boxes
/// and editable combo boxes, matching the behavior of the commit message rich text box.
/// </summary>
/// <remarks>
/// <para>
/// By default the Win32 edit control only breaks on whitespace, so Ctrl+Left/Right
/// jumps over whole paths or identifiers and double-click selects them whole.
/// </para>
/// <para>
/// '_', '+' and '-' are treated as word characters, matching <c>SpellCheckerHelper.IsSeparator</c>
/// used elsewhere for double-click word selection and autocomplete.
/// </para>
/// </remarks>
public static class TextBoxWordBreakExtensions
{
    /// <summary>
    ///  Walks the descendants of <paramref name="control"/> and enables punctuation-aware
    ///  word selection on every plain <see cref="TextBoxBase"/> (excluding
    ///  <see cref="RichTextBox"/>) and editable <see cref="ComboBox"/> found.
    /// </summary>
    /// <remarks>
    ///  After calling this, Ctrl+Left/Right and Ctrl+Shift+Left/Right stop at punctuation
    ///  rather than only at whitespace, double-click selects the run between punctuation
    ///  boundaries, and triple-click selects the entire text. Safe to call more than once
    ///  for the same control (handlers are detached before being re-attached). Called from
    ///  shared form bases such as <c>GitExtensionsFormBase</c>; individual dialogs only need
    ///  to call it themselves when they do not inherit from one.
    /// </remarks>
    public static void EnableProperWordBoundaries(this Control control)
    {
        ArgumentNullException.ThrowIfNull(control);

        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        // RichTextBox (such as the commit message editor) is intentionally excluded: the
        // underlying RichEdit control already breaks on punctuation and selects a line on
        // triple-click, so it needs none of this and that behavior must not be overridden.
        foreach (Control textInput in control.FindDescendants().Where(c => c is ComboBox || c is TextBoxBase and not RichTextBox))
        {
            // Detach first so it is safe to call this for both a form and a control it hosts
            // without handling the keystroke twice.
            textInput.KeyDown -= HandleKeyDown;
            textInput.MouseDown -= HandleMouseDown;
            textInput.MouseUp -= HandleMouseUp;
            textInput.Disposed -= HandleDisposed;

            textInput.KeyDown += HandleKeyDown;
            textInput.MouseDown += HandleMouseDown;
            textInput.MouseUp += HandleMouseUp;
            textInput.Disposed += HandleDisposed;

            if (textInput is ComboBox comboBox)
            {
                // A ComboBox's editable text lives in a separate child Win32 edit control,
                // so it needs its own double-click handling (see HookComboBoxEditControl).
                HookComboBoxEditControl(comboBox);
            }
        }
    }

    // Tracks the fixed end (anchor) of the selection so Ctrl+Shift+Left/Right can extend
    // from the correct side. WinForms does not expose which end the caret is on.
    private static readonly ConditionalWeakTable<Control, StrongBox<int>> _anchors = new();

    /// <summary>Keeps the native subclass of each editable ComboBox's child edit control alive.</summary>
    private static readonly ConditionalWeakTable<ComboBox, ComboBoxEditWindow> _comboBoxEditWindows = new();

    private static void HandleDisposed(object? sender, EventArgs e)
    {
        Control control = (Control)sender!;
        control.Disposed -= HandleDisposed;
        control.KeyDown -= HandleKeyDown;
        control.MouseDown -= HandleMouseDown;
        control.MouseUp -= HandleMouseUp;
        _anchors.Remove(control);

        if (control is not ComboBox comboBox)
        {
            return;
        }

        comboBox.HandleCreated -= HandleComboBoxHandleCreated;
        if (_comboBoxEditWindows.TryGetValue(comboBox, out ComboBoxEditWindow? editWindow))
        {
            editWindow.ReleaseHandle();
            _comboBoxEditWindows.Remove(comboBox);
        }
    }

    private static void HookComboBoxEditControl(ComboBox comboBox)
    {
        comboBox.HandleCreated -= HandleComboBoxHandleCreated;
        comboBox.HandleCreated += HandleComboBoxHandleCreated;

        if (comboBox.IsHandleCreated)
        {
            AttachComboBoxEditWindow(comboBox);
        }
    }

    private static void HandleComboBoxHandleCreated(object? sender, EventArgs e)
        => AttachComboBoxEditWindow((ComboBox)sender!);

    private static void AttachComboBoxEditWindow(ComboBox comboBox)
    {
        if (comboBox.DropDownStyle == ComboBoxStyle.DropDownList)
        {
            return;
        }

        NativeMethods.COMBOBOXINFO info = new() { cbSize = (uint)Unsafe.SizeOf<NativeMethods.COMBOBOXINFO>() };
        if (NativeMethods.GetComboBoxInfo(comboBox.Handle, ref info) != Interop.BOOL.TRUE || info.hwndEdit == IntPtr.Zero)
        {
            return;
        }

        if (_comboBoxEditWindows.TryGetValue(comboBox, out ComboBoxEditWindow? editWindow))
        {
            // The ComboBox's handle (and therefore its child edit handle) can be recreated
            // by WinForms; rebind the subclass to the new edit window when that happens.
            if (editWindow.Handle != info.hwndEdit)
            {
                if (editWindow.Handle != IntPtr.Zero)
                {
                    editWindow.ReleaseHandle();
                }

                editWindow.AssignHandle(info.hwndEdit);
            }
        }
        else
        {
            _comboBoxEditWindows.Add(comboBox, new ComboBoxEditWindow(comboBox, info.hwndEdit));
        }
    }

    private static void HandleMouseDown(object? sender, MouseEventArgs e)
    {
        // TextBoxBase suppresses the MouseDoubleClick event (StandardDoubleClick style is off),
        // but MouseDown still fires with Clicks == 2 for the second click. ComboBox is handled
        // separately via its child edit control (see ComboBoxEditWindow).
        if (e.Clicks == 2 && sender is TextBoxBase textBox)
        {
            // The base class has already applied its default whitespace-only word selection,
            // so this replaces it with one that respects punctuation boundaries.
            SelectWordAt(textBox, textBox.Text, textBox.GetCharIndexFromPosition(e.Location));
        }
    }

    private static void SelectWordAt(Control control, string text, int index)
    {
        if (index < 0 || index >= text.Length)
        {
            return;
        }

        (int start, int end) = GetWordAt(text, index);
        SetSelectionStart(control, start);
        SetSelectionLength(control, end - start);
        SetAnchor(control, start);
    }

    private static void HandleMouseUp(object? sender, MouseEventArgs e)
    {
        // A click collapses the selection; the anchor is wherever the caret landed.
        Control control = (Control)sender!;
        if (GetSelectionLength(control) == 0)
        {
            SetAnchor(control, GetSelectionStart(control));
        }
    }

    private static void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (!e.Control || e.Alt || (e.KeyCode != Keys.Left && e.KeyCode != Keys.Right))
        {
            return;
        }

        Control control = (Control)sender!;
        if (control is ComboBox { DropDownStyle: ComboBoxStyle.DropDownList })
        {
            return;
        }

        bool left = e.KeyCode == Keys.Left;
        bool extend = e.Shift;
        string text = control.Text;
        int selectionStart = GetSelectionStart(control);
        int selectionLength = GetSelectionLength(control);

        (int anchor, int caret) = ResolveAnchorAndCaret(control, selectionStart, selectionLength, left);
        int target = left ? FindPreviousBoundary(text, caret) : FindNextBoundary(text, caret);

        e.Handled = true;
        e.SuppressKeyPress = true;

        if (extend)
        {
            SetSelectionStart(control, Math.Min(anchor, target));
            SetSelectionLength(control, Math.Abs(target - anchor));
            SetAnchor(control, anchor);
        }
        else
        {
            SetSelectionStart(control, target);
            SetSelectionLength(control, 0);
            SetAnchor(control, target);
        }
    }

    /// <summary>Finds the start of the word at or before <paramref name="position"/>.</summary>
    private static int FindPreviousBoundary(string text, int position)
    {
        for (int i = Math.Min(position, text.Length) - 1; i > 0; i--)
        {
            if (IsWordStart(text, i))
            {
                return i;
            }
        }

        return 0;
    }

    /// <summary>Finds the start of the next word after <paramref name="position"/>.</summary>
    private static int FindNextBoundary(string text, int position)
    {
        for (int i = position + 1; i < text.Length; i++)
        {
            if (IsWordStart(text, i))
            {
                return i;
            }
        }

        return text.Length;
    }

    /// <summary>
    ///  Returns the <c>[Start, End)</c> range of the run of same-class characters
    ///  containing <paramref name="index"/> (the word, punctuation run or whitespace run).
    /// </summary>
    private static (int Start, int End) GetWordAt(string text, int index)
    {
        if (index < 0 || index >= text.Length)
        {
            return (index, index);
        }

        char type = GetCharType(text[index]);

        int start = index;
        while (start > 0 && GetCharType(text[start - 1]) == type)
        {
            start--;
        }

        int end = index + 1;
        while (end < text.Length && GetCharType(text[end]) == type)
        {
            end++;
        }

        return (start, end);
    }

    /// <summary>
    ///  A boundary is a transition into a non-whitespace character class, mirroring the
    ///  logic in <see cref="ControlHotkeyExtensions.EnableRemoveWordHotkey"/>.
    /// </summary>
    private static bool IsWordStart(string text, int index)
    {
        char current = GetCharType(text[index]);
        return current != ' ' && current != GetCharType(text[index - 1]);
    }

    private static char GetCharType(char c)
    {
        if (char.IsLetterOrDigit(c) || c is '_' or '+' or '-')
        {
            return 'w';
        }

        if (char.IsWhiteSpace(c))
        {
            return ' ';
        }

        // Each distinct punctuation character is its own class so Ctrl+Left/Right stops
        // between e.g. ')' and ':' as well as at letters.
        return c;
    }

    /// <summary>
    ///  Works out which end of the current selection the caret sits on, so Ctrl+Shift+Arrow can
    ///  extend from the correct side. WinForms does not expose this directly.
    /// </summary>
    private static (int Anchor, int Caret) ResolveAnchorAndCaret(Control control, int selectionStart, int selectionLength, bool left)
    {
        if (selectionLength == 0)
        {
            return (selectionStart, selectionStart);
        }

        int selectionEnd = selectionStart + selectionLength;
        int stored = GetAnchor(control);
        if (stored == selectionStart)
        {
            return (selectionStart, selectionEnd);
        }

        if (stored == selectionEnd)
        {
            return (selectionEnd, selectionStart);
        }

        // No reliable anchor (e.g. selection made with Shift+Arrow): grow outward in the
        // direction pressed.
        return left
            ? (selectionEnd, selectionStart)
            : (selectionStart, selectionEnd);
    }

    private static int GetAnchor(Control control)
        => _anchors.TryGetValue(control, out StrongBox<int>? box) ? box.Value : -1;

    private static void SetAnchor(Control control, int value)
    {
        if (_anchors.TryGetValue(control, out StrongBox<int>? box))
        {
            box.Value = value;
        }
        else
        {
            _anchors.Add(control, new StrongBox<int>(value));
        }
    }

    private static int GetSelectionStart(Control control)
        => control switch
        {
            TextBoxBase t => t.SelectionStart,
            ComboBox cb => cb.SelectionStart,
            _ => throw new NotSupportedException()
        };

    private static void SetSelectionStart(Control control, int value)
    {
        switch (control)
        {
            case TextBoxBase t:
                t.SelectionStart = value;
                break;
            case ComboBox cb:
                cb.SelectionStart = value;
                break;
            default:
                throw new NotSupportedException();
        }
    }

    private static int GetSelectionLength(Control control)
        => control switch
        {
            TextBoxBase t => t.SelectionLength,
            ComboBox cb => cb.SelectionLength,
            _ => throw new NotSupportedException()
        };

    private static void SetSelectionLength(Control control, int value)
    {
        switch (control)
        {
            case TextBoxBase t:
                t.SelectionLength = value;
                break;
            case ComboBox cb:
                cb.SelectionLength = value;
                break;
            default:
                throw new NotSupportedException();
        }
    }

    /// <summary>
    ///  Subclasses the child edit control of an editable <see cref="ComboBox"/> to apply
    ///  punctuation-aware word selection on double-click, which the ComboBox itself does not
    ///  surface (it has no <c>GetCharIndexFromPosition</c> and raises no usable mouse events
    ///  for its edit area).
    /// </summary>
    private sealed class ComboBoxEditWindow : NativeWindow
    {
        private readonly ComboBox _comboBox;

        public ComboBoxEditWindow(ComboBox comboBox, IntPtr editHandle)
        {
            _comboBox = comboBox;
            AssignHandle(editHandle);
        }

        protected override void WndProc(ref Message m)
        {
            // Let the edit control apply its default selection first, then refine it.
            base.WndProc(ref m);

            if (m.Msg == NativeMethods.WM_LBUTTONDBLCLK)
            {
                // m.LParam holds the click position in the edit control's client coordinates.
                int charPos = NativeMethods.SendMessageW(Handle, NativeMethods.EM_CHARFROMPOS, IntPtr.Zero, m.LParam).ToInt32();
                int index = unchecked((short)(charPos & 0xFFFF));
                SelectWordAt(_comboBox, _comboBox.Text, index);
            }
        }
    }

#pragma warning disable S3218 // TestAccessor mirrors private members under their original names so tests read naturally.
    internal static class TestAccessor
    {
        public static int FindPreviousBoundary(string text, int position) =>
            TextBoxWordBreakExtensions.FindPreviousBoundary(text, position);

        public static int FindNextBoundary(string text, int position) =>
            TextBoxWordBreakExtensions.FindNextBoundary(text, position);

        public static (int Start, int End) GetWordAt(string text, int index) =>
            TextBoxWordBreakExtensions.GetWordAt(text, index);
    }
#pragma warning restore S3218
}

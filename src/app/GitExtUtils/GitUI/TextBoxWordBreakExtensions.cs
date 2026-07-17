using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GitUI;

/// <summary>
///  Makes Ctrl+Left/Right, Ctrl+Shift+Left/Right and double-click word selection treat
///  punctuation such as '/', ':', '.', '(' and ')' as word boundaries in plain text boxes
///  and editable combo boxes, matching the behavior of the commit message rich text box.
///  Triple-click selects the whole text.
/// </summary>
/// <remarks>
/// <para>
///  By default the Win32 edit control only breaks on whitespace, so Ctrl+Left/Right
///  jumps over whole paths or identifiers and double-click selects them whole.
/// </para>
/// <para>
///  '_', '+' and '-' are treated as word characters, matching <c>SpellCheckerHelper.IsSeparator</c>
///  used elsewhere for double-click word selection and autocomplete.
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

    /// <summary>
    ///  Per-control state: the fixed end (anchor) of the selection so Ctrl+Shift+Left/Right can
    ///  extend from the correct side (WinForms does not expose which end the caret is on), plus
    ///  the time and location of the last double-click used to detect a triple-click.
    /// </summary>
    private static readonly ConditionalWeakTable<Control, WordSelectionState> _state = new();

    /// <summary>Keeps the native subclass of each editable ComboBox's child edit control alive.</summary>
    private static readonly ConditionalWeakTable<ComboBox, ComboBoxEditWindow> _comboBoxEditWindows = new();

    private sealed class WordSelectionState
    {
        public int Anchor { get; set; }
        public long LastDoubleClickTicks { get; set; }
        public Point LastDoubleClickLocation { get; set; }

        /// <summary>
        ///  The selection the current click gesture intends (caret for a single click, word for a
        ///  double-click, whole text for a triple-click), reasserted on the following mouse-up so it
        ///  wins over the native edit control's drag-extend (see <see cref="HandleMouseUp"/>).
        ///  <c>-1</c> when there is nothing to reassert.
        /// </summary>
        public int PendingStart { get; set; } = -1;
        public int PendingLength { get; set; }

        /// <summary>Mouse-down location of the pending gesture, to tell a stationary click from a drag.</summary>
        public Point PendingDownLocation { get; set; }
    }

    private static void HandleDisposed(object? sender, EventArgs e)
    {
        Control control = (Control)sender!;
        control.Disposed -= HandleDisposed;
        control.KeyDown -= HandleKeyDown;
        control.MouseDown -= HandleMouseDown;
        control.MouseUp -= HandleMouseUp;
        _state.Remove(control);

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
        if (sender is not TextBoxBase textBox)
        {
            return;
        }

        if (e.Clicks == 2)
        {
            // The base class has already applied its default whitespace-only word selection,
            // so this replaces it with one that respects punctuation boundaries.
            (int Start, int Length) word = SelectWordAt(textBox, textBox.Text, textBox.GetCharIndexFromPosition(e.Location));
            SetPendingSelection(textBox, word.Start, word.Length, e.Location);
            RecordDoubleClick(textBox, e.Location);
        }
        else if (e.Clicks == 1 && IsTripleClick(textBox, e.Location))
        {
            textBox.SelectAll();
            SetAnchor(textBox, 0);
            SetPendingSelection(textBox, 0, textBox.TextLength, e.Location);
        }
        else if (e.Clicks == 1)
        {
            // Plain click: remember the caret target so a native drag-extend from a stale anchor
            // (e.g. one left at 0 by a prior Select All) can be collapsed back on mouse-up.
            SetPendingSelection(textBox, textBox.GetCharIndexFromPosition(e.Location), 0, e.Location);
        }
    }

    /// <summary>
    ///  Selects the punctuation-delimited word at <paramref name="index"/> and returns the
    ///  applied <c>[Start, Length]</c>, or <c>(-1, 0)</c> if <paramref name="index"/> is out of range.
    /// </summary>
    private static (int Start, int Length) SelectWordAt(Control control, string text, int index)
    {
        if (index < 0 || index >= text.Length)
        {
            return (-1, 0);
        }

        (int start, int end) = GetWordAt(text, index);
        SetSelectionStart(control, start);
        SetSelectionLength(control, end - start);
        SetAnchor(control, start);
        return (start, end - start);
    }

    private static void HandleMouseUp(object? sender, MouseEventArgs e)
    {
        Control control = (Control)sender!;

        // While the button is held, the native edit control drag-extends the selection from its
        // anchor on any WM_MOUSEMOVE: after a double-click that grows it back to the whole
        // whitespace-delimited run, and after a Select All (anchor left at 0) it grows it to
        // [0, cursor]. Reasserting the gesture's intended selection here, on its final event,
        // makes our selection win regardless of that or any event-ordering race.
        if (TryReapplyPendingSelection(control, e.Location))
        {
            return;
        }

        // A click collapses the selection; the anchor is wherever the caret landed.
        if (GetSelectionLength(control) == 0)
        {
            SetAnchor(control, GetSelectionStart(control));
        }
    }

    private static void SetPendingSelection(Control control, int start, int length, Point downLocation)
    {
        if (start < 0)
        {
            return;
        }

        WordSelectionState state = _state.GetOrCreateValue(control);
        state.PendingStart = start;
        state.PendingLength = length;
        state.PendingDownLocation = downLocation;
    }

    /// <summary>
    ///  Reasserts the selection the current click gesture intended (unless the pointer moved far
    ///  enough since mouse-down to be a genuine drag) and consumes it. Returns whether a pending
    ///  selection was present.
    /// </summary>
    /// <remarks>
    ///  <paramref name="upLocation"/> is the mouse-up position, in the same coordinate space as the
    ///  recorded mouse-down. Beyond the drag threshold the user is drag-selecting, so the native
    ///  selection is left untouched. A plain click that placed a collapsed caret is likewise left
    ///  alone; only a selection the native drag-extend actually corrupted is restored.
    /// </remarks>
    private static bool TryReapplyPendingSelection(Control control, Point upLocation)
    {
        if (!_state.TryGetValue(control, out WordSelectionState? state) || state.PendingStart < 0)
        {
            return false;
        }

        int start = state.PendingStart;
        int length = state.PendingLength;
        Point down = state.PendingDownLocation;
        state.PendingStart = -1;

        Size slop = SystemInformation.DragSize;
        if (Math.Abs(upLocation.X - down.X) > slop.Width || Math.Abs(upLocation.Y - down.Y) > slop.Height)
        {
            return true;
        }

        int currentStart = GetSelectionStart(control);
        int currentLength = GetSelectionLength(control);

        // A plain click intends a collapsed caret; leave the native caret where it landed (only
        // its selection would be reasserted, and there is none), just record the anchor there.
        if (length == 0 && currentLength == 0)
        {
            SetAnchor(control, currentStart);
            return true;
        }

        if (currentStart != start || currentLength != length)
        {
            SetSelectionStart(control, start);
            SetSelectionLength(control, length);
        }

        SetAnchor(control, start);
        return true;
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
        => _state.TryGetValue(control, out WordSelectionState? state) ? state.Anchor : -1;

    private static void SetAnchor(Control control, int value)
        => _state.GetOrCreateValue(control).Anchor = value;

    private static void RecordDoubleClick(Control control, Point location)
    {
        WordSelectionState state = _state.GetOrCreateValue(control);
        state.LastDoubleClickTicks = Environment.TickCount64;
        state.LastDoubleClickLocation = location;
    }

    /// <summary>
    ///  Windows has no triple-click message, so a click that lands shortly after a double-click
    ///  and near the same spot is treated as the third click. The recorded double-click is
    ///  consumed so a further click does not re-trigger.
    /// </summary>
    private static bool IsTripleClick(Control control, Point location)
    {
        if (!_state.TryGetValue(control, out WordSelectionState? state) || state.LastDoubleClickTicks == 0)
        {
            return false;
        }

        long elapsed = Environment.TickCount64 - state.LastDoubleClickTicks;
        Size slop = SystemInformation.DoubleClickSize;
        bool isTripleClick = elapsed >= 0
            && elapsed <= SystemInformation.DoubleClickTime
            && Math.Abs(location.X - state.LastDoubleClickLocation.X) <= slop.Width
            && Math.Abs(location.Y - state.LastDoubleClickLocation.Y) <= slop.Height;

        if (isTripleClick)
        {
            state.LastDoubleClickTicks = 0;
        }

        return isTripleClick;
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

            switch (m.Msg)
            {
                case NativeMethods.WM_LBUTTONDBLCLK:
                {
                    (int Start, int Length) word = SelectWordAt(_comboBox, _comboBox.Text, CharIndexFromPosition(m.LParam));
                    SetPendingSelection(_comboBox, word.Start, word.Length, m.LParam.ToPoint());
                    RecordDoubleClick(_comboBox, m.LParam.ToPoint());
                    break;
                }

                case NativeMethods.WM_LBUTTONUP:
                {
                    // Reassert the click gesture's selection over the edit control's drag-extend
                    // (see HandleMouseUp).
                    TryReapplyPendingSelection(_comboBox, m.LParam.ToPoint());
                    break;
                }

                case NativeMethods.WM_LBUTTONDOWN when IsTripleClick(_comboBox, m.LParam.ToPoint()):
                {
                    _comboBox.SelectAll();
                    SetAnchor(_comboBox, 0);
                    SetPendingSelection(_comboBox, 0, _comboBox.Text.Length, m.LParam.ToPoint());
                    break;
                }

                case NativeMethods.WM_LBUTTONDOWN:
                {
                    // Plain click: remember the caret target so a native drag-extend from a stale
                    // anchor (e.g. one left at 0 by a prior Select All) is collapsed back on mouse-up.
                    SetPendingSelection(_comboBox, CharIndexFromPosition(m.LParam), 0, m.LParam.ToPoint());
                    break;
                }
            }
        }

        /// <summary>Maps a packed click position (an <c>m.LParam</c>, in edit-control client coordinates) to a character index.</summary>
        private int CharIndexFromPosition(IntPtr packedPosition)
        {
            int charPos = NativeMethods.SendMessageW(Handle, NativeMethods.EM_CHARFROMPOS, IntPtr.Zero, packedPosition).ToInt32();
            return unchecked((short)(charPos & 0xFFFF));
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

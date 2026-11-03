using Avalonia.Controls;
using Avalonia.Input;

namespace GitUI;

/// <summary>
///  Combines <see cref="WaitCursorScope"/> with disabling a <see cref="Window"/> for the duration of the scope.
/// </summary>
/// <remarks>
///  Usage is:
///  <code>
///  using (FormBusyScope.Enter(form))
///  {
///      // perform UI activity
///  }
///  </code>
///  This pattern ensures:
///  <list type="bullet">
///  <item>The form is always re-enabled when the block exits, whether by fall-through, return or exception.</item>
///  <item>The wait cursor behavior of <see cref="WaitCursorScope"/> is preserved, including nested-scope handling.</item>
///  </list>
/// </remarks>
public readonly struct FormBusyScope : IDisposable
{
    /// <summary>
    ///  Starts a new scope, disabling <paramref name="form"/> and setting the mouse cursor to <see cref="StandardCursorType.Wait"/>.
    /// </summary>
    /// <param name="form">The form to disable for the duration of the scope.</param>
    /// <param name="cursor">The cursor to display; defaults to <see cref="StandardCursorType.Wait"/>.</param>
    public static FormBusyScope Enter(Window form, Cursor? cursor = null)
    {
        bool wasEnabled = form.IsEnabled;
        form.IsEnabled = false;
        WaitCursorScope waitCursor = WaitCursorScope.Enter(cursor, form);
        return new FormBusyScope(form, wasEnabled, waitCursor);
    }

    private readonly Window _form;
    private readonly bool _wasEnabled;
    private readonly WaitCursorScope _waitCursor;

    private FormBusyScope(Window form, bool wasEnabled, WaitCursorScope waitCursor)
    {
        _form = form;
        _wasEnabled = wasEnabled;
        _waitCursor = waitCursor;
    }

    /// <summary>
    /// Restores the cursor and re-enables <see cref="_form"/>.
    /// </summary>
    public void Dispose()
    {
        _waitCursor.Dispose();
        _form.IsEnabled = _wasEnabled;
    }
}

using Avalonia.Controls;
using Avalonia.Input;

namespace GitUI;

/// <summary>
///  Combines <see cref="WaitCursorScope"/> with disabling a window for the duration of the scope.
/// </summary>
/// <remarks>
///  Twin of GitUI/FormBusyScope.cs. Usage is:
///  <code>
///  using (FormBusyScope.Enter(this))
///  {
///      // perform UI activity
///  }
///  </code>
///  This pattern ensures:
///  <list type="bullet">
///  <item>The window is always re-enabled when the block exits, whether by fall-through,
///  return or exception.</item>
///  <item>The wait cursor behavior of <see cref="WaitCursorScope"/> is preserved,
///  including nested-scope handling.</item>
///  </list>
/// </remarks>
public readonly struct FormBusyScope : IDisposable
{
    /// <summary>
    ///  Starts a new scope, disabling <paramref name="form"/> and setting a wait cursor.
    /// </summary>
    /// <param name="form">The window to disable for the duration of the scope.</param>
    /// <param name="cursor">The cursor to display; defaults to the platform wait cursor.</param>
    public static FormBusyScope Enter(Window form, Cursor? cursor = null)
    {
        bool wasEnabled = form.IsEnabled;
        form.IsEnabled = false;
        WaitCursorScope waitCursor = WaitCursorScope.Enter(cursor);
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
    /// Restores the cursor and re-enables the window.
    /// </summary>
    public void Dispose()
    {
        _waitCursor.Dispose();
        _form.IsEnabled = _wasEnabled;
    }
}

namespace GitUI;

/// <summary>
///  Combines <see cref="WaitCursorScope"/> with disabling a <see cref="Form"/> for the duration of the scope.
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
///  <item>The form is always re-enabled when the block exits, whether by fall-through, return or exception,
///  unless the form was already disposed.</item>
///  <item>The wait cursor behavior of <see cref="WaitCursorScope"/> is preserved, including nested-scope handling.</item>
///  </list>
/// </remarks>
public readonly struct FormBusyScope : IDisposable
{
    /// <summary>
    ///  Starts a new scope, disabling <paramref name="form"/> and setting the mouse cursor to <see cref="Cursors.WaitCursor"/>.
    /// </summary>
    /// <param name="form">The form to disable for the duration of the scope.</param>
    /// <param name="cursor">The cursor to display; defaults to <see cref="Cursors.WaitCursor"/>.</param>
    public static FormBusyScope Enter(Form form, Cursor? cursor = null)
    {
        bool wasEnabled = form.Enabled;
        form.Enabled = false;
        WaitCursorScope waitCursor = WaitCursorScope.Enter(cursor);
        return new FormBusyScope(form, wasEnabled, waitCursor);
    }

    private readonly Form _form;
    private readonly bool _wasEnabled;
    private readonly WaitCursorScope _waitCursor;

    private FormBusyScope(Form form, bool wasEnabled, WaitCursorScope waitCursor)
    {
        _form = form;
        _wasEnabled = wasEnabled;
        _waitCursor = waitCursor;
    }

    /// <summary>
    /// Restores the cursor and re-enables <see cref="_form"/> if it has not been disposed.
    /// </summary>
    public void Dispose()
    {
        _waitCursor.Dispose();

        if (!_form.IsDisposed)
        {
            _form.Enabled = _wasEnabled;
        }
    }
}

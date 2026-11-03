using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;

namespace GitUI;

/// <summary>
/// Sets the mouse cursor to <see cref="StandardCursorType.Wait"/> to indicate UI activity over some lexical scope.
/// </summary>
/// <remarks>
/// Usage is:
/// <code>
/// using (WaitCursorScope.Enter())
/// {
///     // perform UI activity
/// }
/// </code>
/// This pattern ensures:
/// <list type="bullet">
/// <item>The cursor is always restored when the block exits, whether by fall-through, return or exception.</item>
/// <item>Nested scopes work properly. That is, an inner scope doesn't reset the mouse when it exits.
/// Only the top-most scope will fully restore the mouse cursor.</item>
/// </list>
/// </remarks>
public readonly struct WaitCursorScope : IDisposable
{
    /// <summary>
    /// Starts a new scope, recording window cursors and setting the mouse cursor to <see cref="StandardCursorType.Wait"/>.
    /// </summary>
    public static WaitCursorScope Enter(Cursor? cursor = null)
        => Enter(cursor, requiredWindow: null);

    internal static WaitCursorScope Enter(Cursor? cursor, Window? requiredWindow)
    {
        List<(Window Window, Cursor? Cursor)> cursorsAtStartOfScope = [];
        Cursor waitCursor = cursor ?? new Cursor(StandardCursorType.Wait);

        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (Window window in desktop.Windows)
            {
                cursorsAtStartOfScope.Add((window, window.Cursor));
                window.Cursor = waitCursor;
            }
        }

        if (requiredWindow is not null
            && cursorsAtStartOfScope.All(item => item.Window != requiredWindow))
        {
            cursorsAtStartOfScope.Add((requiredWindow, requiredWindow.Cursor));
            requiredWindow.Cursor = waitCursor;
        }

        return new WaitCursorScope(cursorsAtStartOfScope);
    }

    private readonly IReadOnlyList<(Window Window, Cursor? Cursor)> _cursorsAtStartOfScope;

    private WaitCursorScope(IReadOnlyList<(Window Window, Cursor? Cursor)> cursorsAtStartOfScope)
    {
        _cursorsAtStartOfScope = cursorsAtStartOfScope;
    }

    /// <summary>
    /// Restores window cursors to the values captured in the constructor.
    /// </summary>
    public void Dispose()
    {
        foreach ((Window window, Cursor? cursorAtStartOfScope) in _cursorsAtStartOfScope)
        {
            window.Cursor = cursorAtStartOfScope;
        }
    }
}

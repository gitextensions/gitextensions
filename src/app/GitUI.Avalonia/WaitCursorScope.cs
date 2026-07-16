using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;

namespace GitUI;

/// <summary>
/// Sets the mouse cursor to a wait cursor to indicate UI activity over some lexical scope.
/// </summary>
/// <remarks>
/// Twin of GitUI/WaitCursorScope.cs. WinForms tracks one global <c>Cursor.Current</c>;
/// Avalonia only has per-control cursors, so this scope swaps the cursor of every open
/// window and restores the captured values on dispose. Nesting works like the original:
/// an inner scope captures the outer scope's wait cursor and restores back to it, and
/// only the top-most scope fully restores the original cursors.
/// </remarks>
public readonly struct WaitCursorScope : IDisposable
{
    /// <summary>
    /// Starts a new scope, recording each open window's cursor and setting a wait cursor.
    /// </summary>
    public static WaitCursorScope Enter(Cursor? cursor = null)
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

        return new WaitCursorScope(cursorsAtStartOfScope);
    }

    private readonly IReadOnlyList<(Window Window, Cursor? Cursor)> _cursorsAtStartOfScope;

    private WaitCursorScope(IReadOnlyList<(Window Window, Cursor? Cursor)> cursorsAtStartOfScope)
    {
        _cursorsAtStartOfScope = cursorsAtStartOfScope;
    }

    /// <summary>
    /// Restores the window cursors to the values captured in the constructor.
    /// </summary>
    public void Dispose()
    {
        foreach ((Window window, Cursor? cursorAtStartOfScope) in _cursorsAtStartOfScope)
        {
            window.Cursor = cursorAtStartOfScope;
        }
    }
}

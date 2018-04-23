using System;
using System.Windows.Forms;

namespace GitUI
{
    /// <summary>
    /// Sets the mouse cursor to <see cref="Cursors.WaitCursor"/> to indicate UI activity over some lexical scope.
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
        /// Starts a new scope, recording <see cref="Cursor.Current"/> and setting the mouse cursor to <see cref="Cursors.WaitCursor"/>.
        /// </summary>
        public static WaitCursorScope Enter(Cursor cursor = null)
        {
            var cursorAtStartOfScope = Cursor.Current;

            Cursor.Current = cursor ?? Cursors.WaitCursor;

            return new WaitCursorScope(cursorAtStartOfScope);
        }

        private readonly Cursor _cursorAtStartOfScope;

        private WaitCursorScope(Cursor cursorAtStartOfScope)
        {
            _cursorAtStartOfScope = cursorAtStartOfScope;
        }

        /// <summary>
        /// Restores <see cref="Cursor.Current"/> to the value captured in the constructor.
        /// </summary>
        public void Dispose()
        {
            Cursor.Current = _cursorAtStartOfScope;
        }
    }
}
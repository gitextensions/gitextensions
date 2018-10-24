using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace GitUI.Editor
{
    public class SelectedLineEventArgs : EventArgs
    {
        public SelectedLineEventArgs(int selectedLine)
        {
            SelectedLine = selectedLine;
        }

        public int SelectedLine { get; }
    }

    public interface IFileViewer
    {
        event MouseEventHandler MouseMove;
        event EventHandler MouseEnter;
        event EventHandler MouseLeave;
        event EventHandler TextChanged;
        event EventHandler ScrollPosChanged;
        event EventHandler<SelectedLineEventArgs> SelectedLineChanged;
        event KeyEventHandler KeyDown;
        event KeyEventHandler KeyUp;
        event EventHandler DoubleClick;

        void EnableScrollBars(bool enable);
        void Find();
        Task FindNextAsync(bool searchForwardOrOpenWithDifftool);

        string GetText();
        void SetText([NotNull] string text, [CanBeNull] Action openWithDifftool, bool isDiff = false);
        void SetHighlighting([NotNull] string syntax);
        void SetHighlightingForFile(string filename);
        void HighlightLine(int line, Color color);
        void HighlightLines(int startLine, int endLine, Color color);
        void ClearHighlighting();
        string GetSelectedText();
        int GetSelectionPosition();
        int GetSelectionLength();
        void AddPatchHighlighting();
        Action OpenWithDifftool { get; }
        int ScrollPos { get; set; }

        bool ShowLineNumbers { get; set; }
        bool ShowEOLMarkers { get; set; }
        bool ShowSpaces { get; set; }
        bool ShowTabs { get; set; }
        int VRulerPosition { get; set; }
        bool IsReadOnly { get; set; }
        bool Visible { get; set; }

        int FirstVisibleLine { get; set; }
        int GetLineFromVisualPosY(int visualPosY);
        int LineAtCaret { get; set; }
        string GetLineText(int line);
        int TotalNumberOfLines { get; }

        /// <summary>
        /// positions to the given line number
        /// </summary>
        /// <param name="lineNumber">1..MaxLineNumber</param>
        void GoToLine(int lineNumber);
        int MaxLineNumber { get; }

        Font Font { get; set; }

        void SetFileLoader(GetNextFileFnc fileLoader);
    }
}

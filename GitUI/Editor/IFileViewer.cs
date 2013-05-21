using System;
using System.Drawing;
using System.Windows.Forms;

namespace GitUI.Editor
{
    public delegate void SelectedLineChangedEventHandler(object sender, int selectedLine);

    public interface IFileViewer
    {
        event MouseEventHandler MouseMove;
        event EventHandler MouseEnter;
        event EventHandler MouseLeave;
        event EventHandler TextChanged;
        event EventHandler ScrollPosChanged;
        event SelectedLineChangedEventHandler SelectedLineChanged;
        event KeyEventHandler KeyDown;
        event EventHandler DoubleClick;

        void EnableScrollBars(bool enable);
        void Find();

        string GetText();
        void SetText(string text);
        void SetHighlighting(string syntax);
        void SetHighlightingForFile(string filename);
        void HighlightLine(int line, Color color);
        void HighlightLines(int startLine, int endLine, Color color);
        void ClearHighlighting();
        string GetSelectedText();
        int GetSelectionPosition();
        int GetSelectionLength();
        void AddPatchHighlighting();
        int ScrollPos { get; set; }

        bool ShowLineNumbers { get; set; }
        bool ShowEOLMarkers { get; set; }
        bool ShowSpaces { get; set; }
        bool ShowTabs { get; set; }
        bool IsReadOnly { get; set; }
        bool Visible { get; set; }
        
        int FirstVisibleLine { get; set; }
        int GetLineFromVisualPosY(int visualPosY);
        int LineAtCaret { get; }
        string GetLineText(int line);
        int TotalNumberOfLines { get; }
        //lineNumber is 0 based
        void GoToLine(int lineNumber);
        Font Font { get; set; }
        void FocusTextArea();

        void SetFileLoader(Func<bool, Tuple<int, string>> fileLoader);
    }
}

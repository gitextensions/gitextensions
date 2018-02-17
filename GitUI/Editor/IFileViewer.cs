﻿using System;
using System.Drawing;
using System.Windows.Forms;

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
        event EventHandler DoubleClick;

        void EnableScrollBars(bool enable);
        void Find();

        string GetText();
        void SetText(string text, bool isDiff = false);
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
        int VRulerPosition { get; set; }
        bool IsReadOnly { get; set; }
        bool Visible { get; set; }

        int FirstVisibleLine { get; set; }
        int GetLineFromVisualPosY(int visualPosY);
        int LineAtCaret { get; }
        string GetLineText(int line);
        int TotalNumberOfLines { get; }
        //lineNumber is 0 based
        void GoToLine(int lineNumber);

        /// <summary>
        /// Indicates if the Goto line UI is applicable or not.
        /// Code-behind goto line function is always availabe, so we can goto next diff section.
        /// </summary>
        bool IsGotoLineUIApplicable();
        Font Font { get; set; }
        void FocusTextArea();

        void SetFileLoader(Func<bool, Tuple<int, string>> fileLoader);
    }
}

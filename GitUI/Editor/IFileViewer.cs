using System;
using System.Windows.Forms;
using System.Drawing;
namespace GitUI.Editor
{
    public interface IFileViewer
    {
        event MouseEventHandler MouseMove;
        event EventHandler MouseLeave;
        event EventHandler TextChanged;
        event EventHandler ScrollPosChanged;
        event KeyEventHandler KeyDown;

        void EnableScrollBars(bool enable);

        string GetText();
        void SetText(string text);
        void SetHighlighting(string syntax);
        string GetSelectedText();
        void AddPatchHighlighting();
        int ScrollPos { get; set; }

        bool ShowLineNumbers { get; set; }
        bool ShowEOLMarkers { get; set; }
        bool ShowSpaces { get; set; }
        bool ShowTabs { get; set; }
        bool IsReadOnly { get; set; }
        bool Visible { get; set; }
        
        int FirstVisibleLine { get; set; }
        string GetLineText(int line);
        int TotalNumberOfLines { get; }
    }
}

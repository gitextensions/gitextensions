using System;
using System.Windows.Forms;
using System.Drawing;
namespace GitUI.Editor
{
    public delegate void SelectedLineChangedEventHandler(object sender, int selectedLine);

    public interface IFileViewer
    {
        event MouseEventHandler MouseMove;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.SettingsDialog
{
    public interface ISettingsPage
    {
        string Text { get; }

        Control GuiControl { get; }

        void OnPageShown();

        void LoadSettings();

        void SaveSettings();

        IEnumerable<string> GetSearchKeywords();
    }
}

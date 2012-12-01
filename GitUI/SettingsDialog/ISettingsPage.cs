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

        /// <summary>
        /// true if the page cannot properly react to cancel or discard
        /// </summary>
        bool IsInstantApplyPage { get; }

        IEnumerable<string> GetSearchKeywords();
    }
}

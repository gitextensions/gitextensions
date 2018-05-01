using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    /// <summary>
    /// Page to group other pages
    /// </summary>
    public abstract class GroupSettingsPage : Translate, ISettingsPage
    {
        public string Title { get; set; }

        protected GroupSettingsPage(string title)
        {
            Title = title;
            Translator.Translate(this, GitCommands.AppSettings.CurrentTranslation);
        }

        public string GetTitle()
        {
            return Title;
        }

        public Control GuiControl => null;

        public void OnPageShown()
        {
        }

        public void LoadSettings()
        {
        }

        public void SaveSettings()
        {
        }

        public IEnumerable<string> GetSearchKeywords()
        {
            return Array.Empty<string>();
        }

        public bool IsInstantSavePage => false;

        public SettingsPageReference PageReference => new SettingsPageReferenceByType(GetType());
    }
}

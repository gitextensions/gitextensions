﻿using ResourceManager.Translation;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    /// <summary>
    /// Page to group other pages
    /// </summary>
    public abstract class GroupSettingsPage : Translate, ISettingsPage
    {
        public string Title { get; private set; }

        protected GroupSettingsPage(string aTitle)
        {
            Title = aTitle;
            Translator.Translate(this, GitCommands.Properties.Settings.Default.CurrentTranslation);
        }

        public string GetTitle()
        {
            return Title;
        }

        public Control GuiControl { get { return null; } }

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
            return new string[] { };
        }

        public bool IsInstantSavePage
        {
            get { return false; }
        }

        public SettingsPageReference PageReference
        {
            get { return new SettingsPageReferenceByType(GetType()); }
        }

    }
}

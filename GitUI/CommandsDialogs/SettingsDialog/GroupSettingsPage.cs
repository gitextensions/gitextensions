﻿using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    /// <summary>
    /// Page to group other pages
    /// </summary>
    public abstract class GroupSettingsPage : Translate, ISettingsPage
    {
        public string Title { get; set; }

        protected GroupSettingsPage(string aTitle)
        {
            Title = aTitle;
            Translator.Translate(this, AppSettings.CurrentTranslation);
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
            return new string[] { };
        }

        public bool IsInstantSavePage => false;

        public SettingsPageReference PageReference => new SettingsPageReferenceByType(GetType());
    }
}

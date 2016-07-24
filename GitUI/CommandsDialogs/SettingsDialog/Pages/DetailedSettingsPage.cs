using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class DetailedSettingsPage : AutoLayoutSettingsPage
    {
        public DetailedSettingsPage()
        {
            InitializeComponent();
            Text = "Detailed";
            Translate();
        }

        protected override void Init(ISettingsPageHost aPageHost)
        {
            base.Init(aPageHost);
            CreateSettingsControls();
            Translate();
        }

        private DetailedGroup DetailedSettings
        {
            get
            {
                return RepoDistSettingsSet.RepoDistSettings.Detailed;
            }
        }

        private void CreateSettingsControls()
        {
            GroupBoxSettingsLayout main = new GroupBoxSettingsLayout(this, "Browse repository window");
            AddSettingsLayout(main);
            main.AddBoolSetting("Show the Console tab", DetailedSettings.ShowConEmuTab);
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(DetailedSettingsPage));
        }
    }
}

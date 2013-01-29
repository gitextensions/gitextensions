using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.SettingsDialog.Pages
{
    public partial class HotkeysSettingsPage : SettingsPageBase
    {
        public HotkeysSettingsPage()
        {
            InitializeComponent();
            Text = "Hotkeys";
            Translate();
        }

        protected override void OnLoadSettings()
        {
            controlHotkeys.ReloadSettings();
        }

        public override void SaveSettings()
        {
            controlHotkeys.SaveSettings();
        }
    }
}

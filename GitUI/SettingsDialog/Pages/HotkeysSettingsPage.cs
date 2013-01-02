﻿using System;
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
            Translate();

            Text = "Hotkeys";
        }

        public override bool IsInstantSavePage
        {
            get
            {
                return true;
            }
        }

        public override void OnPageShown()
        {
            controlHotkeys.ReloadSettings();
        }
    }
}

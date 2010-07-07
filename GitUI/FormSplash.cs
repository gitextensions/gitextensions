using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ResourceManager.Translation;
using GitCommands;

namespace GitUI
{
    /// <summary>
    /// DO NOT INHERIT FORM GITEXTENIONSFORM SINCE THE SETTINGS ARE NOT YET LOADED WHEN THIS
    /// FORM IS SHOWN! TRANSLATIONS AND COLORED APPLICATION ICONS WILL BREAK!!!!
    /// </summary>
    public partial class FormSplash : Form
    {
        TranslationString version = new TranslationString("Version {0}");

        public FormSplash()
        {
            InitializeComponent(); 
        }

        public void SetAction(string action)
        {
            _actionLabel.Text = action;
            Refresh();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _version.Text = string.Format(version.Text, Settings.GitExtensionsVersionString);
        }
    }
}

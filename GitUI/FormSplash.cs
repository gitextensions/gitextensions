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
    public partial class FormSplash : GitExtensionsForm
    {
        TranslationString version = new TranslationString("Version {0}");

        public FormSplash()
        {
            InitializeComponent(); Translate();
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

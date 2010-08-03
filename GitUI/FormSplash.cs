using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    /// <summary>
    ///   DO NOT INHERIT FORM GITEXTENIONSFORM SINCE THE SETTINGS ARE NOT YET LOADED WHEN THIS
    ///   FORM IS SHOWN! TRANSLATIONS AND COLORED APPLICATION ICONS WILL BREAK!!!!
    /// </summary>
    public partial class FormSplash : Form
    {
        private readonly TranslationString _version = new TranslationString("Version {0}");

        public FormSplash()
        {
            InitializeComponent();

            SetFont();
            _programTitle.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);
        }

        private void SetFont()
        {
            Font = SystemFonts.MessageBoxFont;
        }

        public void SetAction(string action)
        {
            _actionLabel.Text = action;
            Refresh();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _versionLabel.Text = string.Format(_version.Text, Settings.GitExtensionsVersionString);
        }
    }
}
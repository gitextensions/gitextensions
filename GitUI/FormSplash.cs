using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using ResourceManager;

namespace GitUI
{
    /// <summary>
    ///   DO NOT INHERIT FORM GITEXTENIONSFORM SINCE THE SETTINGS ARE NOT YET LOADED WHEN THIS
    ///   FORM IS SHOWN! TRANSLATIONS AND COLORED APPLICATION ICONS WILL BREAK!!!!
    /// </summary>
    public partial class FormSplash : Form
    {
        private readonly TranslationString _version = new TranslationString("Version {0}");

        private static FormSplash instance;

        public static void ShowSplash()
        {
            instance = new FormSplash();
            instance.Show();
        }

        public static void SetAction(string action)
        {
            if (instance != null)
                instance.SetActionText(action);
        }

        public static void HideSplash()
        {
            if (instance != null)
            {
                instance.Dispose();
                instance = null;
            }
        }

        private FormSplash()
        {
            ShowInTaskbar = false;

            InitializeComponent();

            SetFont();
            _NO_TRANSLATE_programTitle.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);
			
           	var image = Lemmings.GetPictureBoxImage(DateTime.Now);
           	pictureBox1.Image = image ?? Properties.Resources.git_extensions_logo_final_128;
        }

        private void SetFont()
        {
            Font = SystemFonts.MessageBoxFont;
        }

        private void SetActionText(string action)
        {
            _NO_TRANSLATE_actionLabel.Text = action;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _NO_TRANSLATE_versionLabel.Text = string.Format(_version.Text, AppSettings.GitExtensionsVersionString);

            if (EnvUtils.RunningOnUnix())
                _NO_TRANSLATE_osLabel.Text = "Unix";
            if (EnvUtils.RunningOnMacOSX())
                _NO_TRANSLATE_osLabel.Text = "MacOSX";
            if (EnvUtils.RunningOnWindows())
                _NO_TRANSLATE_osLabel.Text = "Windows";
        }
    }
}
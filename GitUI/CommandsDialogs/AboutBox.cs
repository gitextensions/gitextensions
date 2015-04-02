using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GitUI.Properties;
using GitUI.CommandsDialogs.AboutBoxDialog;

namespace GitUI.CommandsDialogs
{
    public partial class AboutBox : GitExtensionsForm
    {
        private readonly string[] _all;
        private readonly Random _random;
        private readonly string[] _coders;
        private readonly string[] _translators;
        private readonly string[] _designers;
        private readonly string[] _other;

        public AboutBox()
        {
            _coders = Resources.Coders.Split(',').Select(c => c.Trim()).ToArray();
            _translators = Resources.Translators.Split(',').Select(c => c.Trim()).ToArray();
            _designers = Resources.Designers.Split(',').Select(c => c.Trim()).ToArray();
            _other = Resources.Other.Split(',').Select(c => c.Trim()).ToArray();
            _all = _coders.Concat(_translators).Concat(_designers).Concat(_other).ToArray();
            _random = new Random();

            InitializeComponent();
            Translate();
        }

        private IList<string> Coders
        {
            get { return _coders; }
        }

        private IList<string> Translators
        {
            get { return _translators; }
        }

        private IList<string> Designers
        {
            get { return _designers; }
        }

        private IList<string> Other
        {
            get { return _other; }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WAL2SSDV8ND54&lc=US&item_name=GitExtensions&no_note=1&no_shipping=1&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            Bitmap image = Lemmings.GetPictureBoxImage(DateTime.Now);
            if (image != null)
                logoPictureBox.Image = image;

            thanksTimer_Tick(null, null);
            thanksTimer.Enabled = true;
            thanksTimer.Interval = 500;
            thanksTimer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _NO_TRANSLATE_labelVersionInfo.Text = string.Format("{0}{1}", _NO_TRANSLATE_labelVersionInfo.Text,
                GitCommands.AppSettings.ProductVersion);
        }

        private void thanksTimer_Tick(object sender, EventArgs e)
        {
            _NO_TRANSLATE_thanksToTicker.Text = _all[_random.Next(_all.Length - 1)].Trim();
        }

        private void _NO_TRANSLATE_thanksToTicker_Click(object sender, EventArgs e)
        {
            using (FormContributors formContributors = new FormContributors())
            {
                formContributors.LoadContributors(Coders, Translators,
                    Designers, Other);
                formContributors.ShowDialog(this);
            }
        }
    }
}

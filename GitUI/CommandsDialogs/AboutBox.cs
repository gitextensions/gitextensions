using System;
using System.Drawing;
using GitUI.Properties;
using GitUI.CommandsDialogs.AboutBoxDialog;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class AboutBox : GitExtensionsForm
    {
        public AboutBox()
        {
            _contributersList = string.Join(", ", new []{Coders, Translators,
                Designers, Other})
                .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

            InitializeComponent();
            Translate();
        }

        private string Coders
        {
            get { return Resources.Coders.Replace(Environment.NewLine, " "); }
        }

        private string Translators
        {
            get { return Resources.Translators.Replace(Environment.NewLine, " "); }
        }

        private string Designers
        {
            get { return Resources.Designers.Replace(Environment.NewLine, " "); }
        }

        private string Other
        {
            get { return Resources.Other.Replace(Environment.NewLine, " "); }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void labelVersion_Click(object sender, EventArgs e)
        {

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

        private readonly string[] _contributersList;
        private readonly Random _random = new Random();

        private void thanksTimer_Tick(object sender, EventArgs e)
        {
            _NO_TRANSLATE_thanksToTicker.Text = _contributersList[_random.Next(_contributersList.Length - 1)].Trim();
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

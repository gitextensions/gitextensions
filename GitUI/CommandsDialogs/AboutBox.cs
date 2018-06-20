using System;
using GitUI.CommandsDialogs.AboutBoxDialog;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Properties;

namespace GitUI.CommandsDialogs
{
    public partial class AboutBox : GitExtensionsForm
    {
        public AboutBox()
        {
            _contributersList = string.Join(", ", Coders, Translators, Designers, Other)
                .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

            InitializeComponent();
            Translate();
            this.AdjustForDpiScaling();
        }

        private static string Coders => Resources.Coders.Replace(Environment.NewLine, " ");

        private static string Translators => Resources.Translators.Replace(Environment.NewLine, " ");

        private static string Designers => Resources.Designers.Replace(Environment.NewLine, " ");

        private static string Other => Resources.Other.Replace(Environment.NewLine, " ");

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(FormDonate.DonationUrl);
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
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
                formContributors.LoadContributors(Coders, Translators, Designers);
                formContributors.ShowDialog(this);
            }
        }
    }
}

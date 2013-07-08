using System;
using System.Drawing;
using GitUI.CommandsDialogs.AboutBoxDialog;

namespace GitUI.CommandsDialogs
{
    public partial class AboutBox : GitExtensionsForm
    {
        public AboutBox()
        {
            contributersList = string.Concat(coders, ", ", translators, ", ", designers, ", ", other).Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);

            InitializeComponent(); 
            Translate();
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
                GitCommands.AppSettings.GitExtensionsVersionString);
        }

        //Contributers list 
        private const string coders = "Arkadiy Shapkin, Janusz Białobrzewski, Steffen Forkmann, Jacob Stanley, " +
            "Nick Mayer, Kevin Moore, Davide, Dominique Plante, Grzegorz Pachocki, Seth Behunin, bleis-tift, " +
            "Chris Meaney, Nathanael Schmied, Adrian Codrington, Troels Thomsen, Wilbert van Dolleweerd, " +
            "Tobias Bieniek, Radoslaw Miazio, Stan Angeloff, Matt McCormick, Bjørn Moe, William Swanson, " +
            "Daniel Locantore, Harald Deischinger, Radek Miazio, Stefan Rueckl, Emanuel Henrique do Prado, " +
            "Lukasz Byczynski, Steffen M. Colding-Jørgensen, alexeik, arBmind, mausch, xaro, Xharze, Kim Christensen, " +
            "showell, Daniel Doubrovkine, mdk, Marc Murray, rferriz, Jacek Pasternak, miloja, Ed Starback, Alberto Chiesa, " +
            "Charles Brossollet, Patrick Earl, ultonis, Michael Frenzel, Airat Salikhov, Max Malook, ikke, Simon Walker, " +
            "Arnaud Fabre, Andy Lee, Joe Brown, Rodrigo, John Gietzen, Ralph Haußmann, Rodrigo Fraga, Michael West, " +
            "David Vierra, Mark Pizzolato, Alexander Mueller, marcinmagier, Alexander Puzynia, ferow2k, lynxstv, nitoyon, " +
            "iamxail, Basewq, Edward Brey, Sergey, Nils Fenner, Burim Kameri, Phillip Cohen, Andy Royle, Masanori Tanaka, " +
            "Alex Ford, Arne Janbu, Dan Rigby, pravic, Linquize, Clinton Daniel, Reto Schoening, mabako, Tal952, " +
            "Aviad Pineles, Markus Stein, Marcus Bauer, Nay, Joe Phillips, Cameron Will, Donatas Mačiūnas, Jesse Bartley, " +
            "Dave Brotherstone, Pieter van Ginkel, australiensun, Vincent Gravade, Hiroyuki Sato, Isaac Devine, " +
            "Konstantin Tenzin, Stefan Laut, Jeromy Johnson, Kate von Roeder, Tor Arvid Lund, jberger, kunigaku, Jay Asbury";
        private const string translators = "Gianni Rosa Gallina, Cheng Huang, Floyd Hung, superlongman, rferriz, gor, " +
            "xaro, bleis-tift, Ralph Haußmann, Jasper Chien, Arkadiy Shapkin, ferow2k, Thibault D'Archivio, australiensun, " +
            "Airat Salikhov, Dave Brotherstone, diegoaossas, hogelog, Philippe Miossec, Copro";
        private const string designers = "Andréj Telle, Oliver Friedrich";
        private const string other = "";

        private readonly string[] contributersList;
        private readonly Random random = new Random();

        private void thanksTimer_Tick(object sender, EventArgs e)
        {
            _NO_TRANSLATE_thanksToTicker.Text = contributersList[random.Next(contributersList.Length - 1)].Trim();
        }

        private void _NO_TRANSLATE_thanksToTicker_Click(object sender, EventArgs e)
        {
            using (FormContributors formContributors = new FormContributors())
            {
                formContributors.LoadContributors(coders, translators, designers, other);
                formContributors.ShowDialog(this);
            }
        }
    }
}

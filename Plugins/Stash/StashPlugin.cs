using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace Stash
{
    public class StashPlugin : GitPluginBase
    {
        public const string StashUsername = "Stash Username";
        public const string StashPassword = "Stash Password";

        public override string Description
        {
            get { return "Create Stash Pull Request"; }
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var frm = new StashPullRequestForm(gitUiCommands, base.Settings))
                frm.ShowDialog(gitUiCommands.OwnerForm as IWin32Window);
            return true;
        }

        protected override void RegisterSettings()
        {
            base.RegisterSettings();

            Settings.AddSetting(StashUsername, string.Empty);
            Settings.AddSetting(StashPassword, string.Empty);
        }
    }
}

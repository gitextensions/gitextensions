using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace DeleteUnusedBranches
{
    public class DeleteUnusedBranchesPlugin : GitPluginBase, IGitPluginForRepository
    {
        private StringSetting MergedInBranch = new StringSetting("Branch where all branches should be merged in", "HEAD");
        private NumberSetting<int> DaysOlderThan = new NumberSetting<int>("Delete obsolete branches older than (days)", 30);

        public override string Description
        {
            get { return "Delete obsolete branches"; }
        }

        public override System.Collections.Generic.IEnumerable<ISetting> GetSettings()
        {
            yield return DaysOlderThan;
            yield return MergedInBranch;
        }

        public override bool Execute(GitUIBaseEventArgs gitUiArgs)
        {
            using (var frm = new DeleteUnusedBranchesForm(DaysOlderThan[Settings], MergedInBranch[Settings], gitUiArgs.GitModule, gitUiArgs.GitUICommands, this))
            {
                frm.ShowDialog(gitUiArgs.OwnerForm);
            }

            return true;
        }
    }
}

using System.Collections.Generic;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace FindLargeFiles
{
    public class FindLargeFilesPlugin : GitPluginBase, IGitPluginForRepository
    {
        private NumberSetting<float> SizeLargeFile = new NumberSetting<float>("Find large files bigger than (Mb)", 1);
        public override string Description
        {
            get { return "Find large files"; }
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            //return all settings or introduce implementation based on reflection on GitPluginBase level
            yield return SizeLargeFile;
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var frm = new FindLargeFilesForm(SizeLargeFile[Settings], gitUiCommands))
                frm.ShowDialog(gitUiCommands.OwnerForm);
            return true;
        }
    }
}

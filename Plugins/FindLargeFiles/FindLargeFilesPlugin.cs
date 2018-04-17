using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitUIPluginInterfaces;
using ResourceManager;

namespace FindLargeFiles
{
    [Export(typeof(IGitPlugin))]
    public class FindLargeFilesPlugin : GitPluginBase, IGitPluginForRepository
    {
        public FindLargeFilesPlugin()
        {
            SetNameAndDescription("Find large files");
            Translate();
        }

        private readonly NumberSetting<float> _sizeLargeFile = new NumberSetting<float>("Find large files bigger than (Mb)", 1);

        public override IEnumerable<ISetting> GetSettings()
        {
            // return all settings or introduce implementation based on reflection on GitPluginBase level
            yield return _sizeLargeFile;
        }

        public override bool Execute(GitUIEventArgs gitUiCommands)
        {
            using (var frm = new FindLargeFilesForm(_sizeLargeFile.ValueOrDefault(Settings), gitUiCommands))
            {
                frm.ShowDialog(gitUiCommands.OwnerForm);
            }

            return true;
        }
    }
}

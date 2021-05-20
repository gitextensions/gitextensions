using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitExtensions.Plugins.FindLargeFiles.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions.Plugins.FindLargeFiles
{
    [Export(typeof(IGitPlugin))]
    public class FindLargeFilesPlugin : GitPluginBase, IGitPluginForRepository
    {
        public FindLargeFilesPlugin() : base(true)
        {
            Id = new Guid("5AE20AB1-D677-46C5-ABDB-7874FF5A9296");
            Name = "Find large files";
            Translate();
            Icon = Resources.IconFindLargeFiles;
        }

        private readonly NumberSetting<float> _sizeLargeFile = new("Find large files bigger than (Mb)", 1);

        public override IEnumerable<ISetting> GetSettings()
        {
            // return all settings or introduce implementation based on reflection on GitPluginBase level
            yield return _sizeLargeFile;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            using FindLargeFilesForm frm = new(_sizeLargeFile.ValueOrDefault(Settings), args);
            frm.ShowDialog(args.OwnerForm);

            return true;
        }
    }
}

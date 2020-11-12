using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using FindLargeFiles.Properties;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;

namespace FindLargeFiles
{
    [Export(typeof(IGitPlugin))]
    public sealed class Plugin : IGitPlugin,
        IGitPluginForRepository,
        IGitPluginConfigurable,
        IGitPluginExecutable
    {
        private readonly NumberSetting<float> _sizeLargeFile = new NumberSetting<float>("Find large files bigger than (Mb)", Strings.SizeLargeFile, 1);

        public string Name => "Find large files";

        public string Description => Strings.Description;

        public Image Icon => Images.IconFindLargeFiles;

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public IEnumerable<ISetting> GetSettings()
        {
            // return all settings or introduce implementation based on reflection on GitPluginBase level
            yield return _sizeLargeFile;
        }

        public bool Execute(GitUIEventArgs args)
        {
            using var frm = new FindLargeFilesForm(_sizeLargeFile.ValueOrDefault(SettingsContainer.GetSettingsSource()), args);

            frm.ShowDialog(args.OwnerForm);

            return true;
        }
    }
}

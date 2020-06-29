using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GitCommands;
using GitCommands.Gpg;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.UserControls.GPGKeys
{
    public partial class GPGSecretKeysCombobox : GitModuleControl
    {
        private GPGKeysUIController _uiController;
        public GPGSecretKeysCombobox()
        {
            InitializeComponent();
            InitializeComplete();
        }

        protected override void OnRuntimeLoad()
        {
            _uiController = new GPGKeysUIController(new GitGpgController(() => Module));
            LoadKeys();
            base.OnRuntimeLoad();
        }

        private void LoadKeys()
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                var displayKeys = await _uiController.GetKeysAsync();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                gpgKeyDisplayInfoBindingSource.DataSource = displayKeys;
            });
        }

        private void comboBoxKeys_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxKeys.SelectedIndex != -1)
            {
                Debugger.Log(0, "GpgInfo", $"{comboBoxKeys.SelectedIndex} was selected, which is {comboBoxKeys.SelectedItem}.{Environment.NewLine}");
            }
        }

        private void comboBoxKeys_DropDown(object sender, EventArgs e)
        {
            LoadKeys();
        }

        private IEnumerable<GpgKeyDisplayInfo> CurrentKeys { get => (IEnumerable<GpgKeyDisplayInfo>)gpgKeyDisplayInfoBindingSource.DataSource; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string KeyID
        {
            get => ((GpgKeyDisplayInfo)comboBoxKeys.SelectedItem)?.KeyID ?? "";
            set
            {
                LoadKeys();
                var empty = CurrentKeys.First();
                var matched = CurrentKeys.FirstOrDefault(k => !string.IsNullOrWhiteSpace(value) && k.KeyID.EndsWith(value));
                comboBoxKeys.SelectedItem = matched ?? empty;
            }
        }
    }
}

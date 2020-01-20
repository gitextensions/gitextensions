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

namespace GitUI.UserControls
{
    public partial class GPGSecretKeysComboboxControl : GitModuleControl
    {
        private IGitGpgController _gpgController;
        public GPGSecretKeysComboboxControl()
        {
            InitializeComponent();
            _gpgController = new GitGpgController(() => Module);
            InitializeComplete();
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

        private void LoadKeys()
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                var keys = await _gpgController.GetGpgSecretKeys();
                var displayKeys = keys.Select(k => new GpgKeyDisplayInfo(k.Fingerprint, k.KeyID, k.UserID)).ToArray();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                gpgKeyDisplayInfoBindingSource.DataSource = displayKeys;
            });
        }

        private void GPGSecretKeysComboboxControl_Load(object sender, EventArgs e)
        {
            if (Site != null && Site.DesignMode)
            {
                return;
            }

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

        public class GpgKeyDisplayInfo
        {
            private readonly TranslationString _noKeySelected = new TranslationString("No key selected");
            public GpgKeyDisplayInfo(string fingerprint, string keyID, string userID)
            {
                Fingerprint = fingerprint;
                KeyID = keyID;
                UserID = userID;
            }

            public string Fingerprint { get; set; }
            public string KeyID { get; set; }
            public string UserID { get; set; }
            public string Caption
            {
                get => string.IsNullOrEmpty(UserID) ? _noKeySelected.Text : $"{UserID} ({KeyID})";
            }

            public override string ToString()
            {
                return Caption;
            }
        }
    }
}

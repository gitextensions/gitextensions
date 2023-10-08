using System.ComponentModel;
using System.Diagnostics;
using GitCommands.Git.Gpg;
using GitCommands.Gpg;
using GitUI.UserControls.RevisionGrid;
using Microsoft.VisualStudio.Threading;

namespace GitUI.UserControls.GPGKeys
{
    public partial class GpgSecretKeysSelector : GitModuleControl
    {
        private GPGKeysUIController _uiController;
        private IGPGSecretKeysParser _secretKeysParser;
        private bool _loadedKeys = false;
        private string _selectedKeyID = "";

        public IEnumerable<GpgKeyDisplayInfo> CurrentKeys { get => _loadedKeys ? (IEnumerable<GpgKeyDisplayInfo>)gpgKeyDisplayInfoBindingSource.DataSource : Enumerable.Empty<GpgKeyDisplayInfo>(); }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string KeyID
        {
            get => comboBoxKeys.SelectedIndex > -1 ? ((GpgKeyDisplayInfo)comboBoxKeys.SelectedItem).KeyId ?? "" : "";
            set
            {
                _selectedKeyID = value;
                if (_loadedKeys)
                {
                    var empty = CurrentKeys?.First();
                    var matched = CurrentKeys.Select((k, i) => new { Key = k, Index = i }).FirstOrDefault(k => !string.IsNullOrWhiteSpace(value) && k.Key.KeyId.EndsWith(value));
                    if (matched is not null)
                    {
                        comboBoxKeys.SelectedIndex = matched.Index;
                    }
                    else
                    {
                        comboBoxKeys.SelectedIndex = 0;
                    }
                }
            }
        }

        public bool IsDefaultSelected => _loadedKeys && SelectedIndex >= 0 && ((GpgKeyDisplayInfo)comboBoxKeys.SelectedItem).IsDefault;
        public GpgSecretKeysSelector() : base()
        {
            InitializeComponent();
            InitializeComplete();
        }

        public void Initilize(IGitUICommandsSource uiCommandsSource, IGPGSecretKeysParser secretKeysParser)
        {
            _secretKeysParser = secretKeysParser;
            _uiController = new GPGKeysUIController(new GitGpgController(() => Module, _secretKeysParser));
            UICommandsSource = uiCommandsSource;
        }

        private bool _isLoadingKeys;

        private async Task LoadKeysAsync(bool selectKey = false)
        {
            if (_isLoadingKeys)
            {
                return;
            }

            _isLoadingKeys = true;
            LoadingControl loadingControlLoading = new();
            Controls.Add(loadingControlLoading);
            loadingControlLoading.Visible = true;
            loadingControlLoading.BringToFront();
            loadingControlLoading.IsAnimating = true;

            await ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await TaskScheduler.Default;
                IEnumerable<GpgKeyDisplayInfo> displayKeys = await _uiController.GetKeysAsync();

                displayKeys = Enumerable.Prepend(displayKeys, new GpgKeyDisplayInfo("", "", "", false));
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                gpgKeyDisplayInfoBindingSource.DataSource = displayKeys;

                Controls.Remove(loadingControlLoading);
                loadingControlLoading.Dispose();
                if (selectKey && !string.IsNullOrEmpty(_selectedKeyID))
                {
                    KeyID = _selectedKeyID; // Set KeyID after we know we can get a usable module.  Allows for setting the KeyID and it still apply even if UICommandSource is not set yet.
                }

                _selectedKeyID = "";
                _loadedKeys = true;
                _isLoadingKeys = false;

                OnKeysLoaded(EventArgs.Empty);
            });
        }

        #region Bubble Events
        private void GPGSecretKeysCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxKeys.SelectedIndex != -1)
            {
                Debugger.Log(0, "GpgInfo", $"{comboBoxKeys.SelectedIndex} was selected, which is {comboBoxKeys.SelectedItem}.{Environment.NewLine}");
            }

            OnSelectedIndexChanged(this, EventArgs.Empty);
        }

        private void GPGSecretKeysCombobox_MouseHover(object sender, EventArgs e)
        {
            OnMouseHover(EventArgs.Empty);
        }

        #endregion

        private void GPGSecretKeysCombobox_DropDown(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                var dropDownHost = Parent as ToolStripDropDown; // recursive instead?
                if (dropDownHost != null)
                {
                    // ensure that our parent doesn't close while the dropdown is open
                    dropDownHost.AutoClose = false;
                }
            }

            if (_loadedKeys)
            {
                LoadKeysAsync().FileAndForget();
            }
        }

        private void comboBoxKeys_DropDownClosed(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                var dropDownHost = Parent as ToolStripDropDown; // recursive instead?
                if (dropDownHost != null)
                {
                    dropDownHost.AutoClose = true; // restore the parent's AutoClose preference
                }
            }
        }

        private void ContextMenuActions_Opening(object sender, CancelEventArgs e)
        {
            if (Parent != null)
            {
                var dropDownHost = Parent as ToolStripDropDown; // recursive instead?
                if (dropDownHost != null)
                {
                    // ensure that our parent doesn't close while the dropdown is open
                    dropDownHost.AutoClose = false;
                }
            }
        }

        private void ContextMenuActions_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (Parent != null)
            {
                var dropDownHost = Parent as ToolStripDropDown; // recursive instead?
                if (dropDownHost != null)
                {
                    dropDownHost.AutoClose = true; // restore the parent's AutoClose preference
                    Focus();
                }
            }
        }

        private void ContextMenuActions_Click(object sender, EventArgs e)
        {
            /*
            if (Parent != null)
            {
                var dropDownHost = Parent as ToolStripDropDown; // recursive instead?
                if (dropDownHost != null)
                {
                    dropDownHost.AutoClose = true; // restore the parent's AutoClose preference
                    Focus();
                }
            }
            */
        }

        public void SelectDefaultKey()
        {
            var defaultKey = CurrentKeys?.FirstOrDefault(i => i.IsDefault);
            if (defaultKey != null)
            {
                comboBoxKeys.SelectedItem = defaultKey;
            }
            else
            {
                comboBoxKeys.SelectedIndex = 0;
            }
        }

        public GPGKeysUIController KeysUIController => _uiController;

        protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
        {
            base.OnUICommandsSourceSet(source);
            LoadKeysAsync(!string.IsNullOrEmpty(_selectedKeyID)).FileAndForget();
        }

        public int SelectedIndex { get => comboBoxKeys.SelectedIndex; set => comboBoxKeys.SelectedIndex = value; }

        /// <summary>
        /// Used to get the display text of the combobox.
        /// </summary>
        /// <remarks>
        /// Do NOT set this property.  Value will be ignored.
        /// </remarks>
        public override string Text { get => comboBoxKeys.Text; set => base.Text = value; }
        protected virtual void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedIndexChanged?.Invoke(sender, e);
        }

        protected virtual void OnKeysLoaded(EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            KeysLoaded?.Invoke(this, e);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitGpgController.ClearKeyCache();
            LoadKeysAsync().FileAndForget();

            if (Parent != null)
            {
                var dropDownHost = Parent as ToolStripDropDown; // recursive instead?
                if (dropDownHost != null)
                {
                    dropDownHost.AutoClose = true; // restore the parent's AutoClose preference
                    Focus();
                }
            }
        }

        public event EventHandler SelectedIndexChanged;
        public event EventHandler KeysLoaded;

        protected override void DisposeCustomResources()
        {
            comboBoxKeys.SelectedIndexChanged -= GPGSecretKeysCombobox_SelectedIndexChanged;
            comboBoxKeys.DropDown -= GPGSecretKeysCombobox_DropDown;
            comboBoxKeys.MouseHover -= GPGSecretKeysCombobox_MouseHover;
            comboBoxKeys.Dispose();

            base.DisposeCustomResources();
        }
    }
}

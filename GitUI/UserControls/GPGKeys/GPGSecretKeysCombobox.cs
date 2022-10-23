using System.ComponentModel;
using System.Diagnostics;
using GitCommands.Gpg;

namespace GitUI.UserControls.GPGKeys
{
    public partial class GPGSecretKeysCombobox : GitModuleControl
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
            get => ((GpgKeyDisplayInfo)comboBoxKeys.SelectedItem)?.KeyID ?? "";
            set
            {
                _selectedKeyID = value;
                if (_loadedKeys)
                {
                    var empty = CurrentKeys?.First();
                    var matched = CurrentKeys.FirstOrDefault(k => !string.IsNullOrWhiteSpace(value) && k.KeyID.EndsWith(value));
                    comboBoxKeys.SelectedItem = matched ?? empty;
                }
            }
        }

        public GPGSecretKeysCombobox() : base()
        {
            InitializeComponent();
            InitializeComplete();
        }

        private void InitControllers()
        {
            _secretKeysParser = new GPGSecretKeysParser();
            _uiController = new GPGKeysUIController(new GitGpgController(() => Module, _secretKeysParser));
            LoadKeys();
        }

        private void LoadKeys()
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                var displayKeys = await _uiController.GetKeysAsync();
                displayKeys = Enumerable.Prepend(displayKeys, new GpgKeyDisplayInfo("", "", "", false));
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                gpgKeyDisplayInfoBindingSource.DataSource = displayKeys;
                _loadedKeys = true;
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
            LoadKeys();
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

            InitControllers(); // init when UICommandSource is set.
            if (!string.IsNullOrEmpty(_selectedKeyID))
            {
                KeyID = _selectedKeyID; // Set KeyID after we know we can get a usable module.  Allows for setting the KeyID and it still apply even if UICommandSource is not set yet.
            }
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

        public event EventHandler SelectedIndexChanged;
    }
}

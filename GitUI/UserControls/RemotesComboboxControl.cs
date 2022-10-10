﻿using GitCommands.Remotes;

namespace GitUI.UserControls
{
    public partial class RemotesComboboxControl : GitModuleControl
    {
        public RemotesComboboxControl()
        {
            InitializeComponent();
            InitializeComplete();
            AllowMultiselect = false;
        }

        public string SelectedRemote
        {
            get => comboBoxRemotes.Text;
            set => comboBoxRemotes.Text = value;
        }

        private bool _allowMultiselect;
        public bool AllowMultiselect
        {
            get { return _allowMultiselect; }
            set
            {
                _allowMultiselect = value;
                buttonSelectMultipleRemotes.Visible = _allowMultiselect;
                if (_allowMultiselect)
                {
                    throw new NotImplementedException();
                }
            }
        }

        private void RemotesComboboxControl_Load(object sender, EventArgs e)
        {
            if (Site is not null && Site.DesignMode)
            {
                return;
            }

            ConfigFileRemoteSettingsManager remotesManager = new(() => Module);
            comboBoxRemotes.DataSource = remotesManager.LoadRemotes(false).Select(x => x.Name).ToList();
        }
    }
}

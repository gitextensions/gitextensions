namespace GitUI.UserControls.GPGKeys
{
    public class ToolStripGPGSecretKeysComboBox : ToolStripControlHost
    {
        private GPGSecretKeysCombobox cmbo { get => (GPGSecretKeysCombobox)Control; }
        #region Bubble Events

        private void cmbo_SelectedIndexChnaged(object sender, EventArgs e)
        {
            OnSelectedIndexChanged(this, EventArgs.Empty);
        }

        #endregion
        public ToolStripGPGSecretKeysComboBox() : base(new GPGSecretKeysCombobox())
        {
            AutoSize = false;
            AutoToolTip = false; // Use TooltipText for tooltip
            cmbo.Size = Size;
            cmbo.Dock = DockStyle.Fill;
            cmbo.SelectedIndexChanged += new System.EventHandler(cmbo_SelectedIndexChnaged);
        }

        public string KeyID { get => cmbo.KeyID; set => cmbo.KeyID = value; }
        public GPGKeysUIController KeysUIController => cmbo.KeysUIController;
        protected override Size DefaultSize { get => new(230, 23); }
        public IEnumerable<GpgKeyDisplayInfo> CurrentKeys { get => cmbo.CurrentKeys; }
        public int SelectedIndex { get => cmbo.SelectedIndex; set => cmbo.SelectedIndex = value; }

        /// <summary>
        /// Used to get the display text of the combobox.
        /// </summary>
        /// <remarks>
        /// Do NOT set this property.  Value will be ignored.
        /// </remarks>
        public override string Text { get => cmbo.Text; set => base.Text = value; }
        public IGitUICommandsSource UICommandsSource { get => cmbo.UICommandsSource; set => cmbo.UICommandsSource = value; }
        protected virtual void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedIndexChanged?.Invoke(sender, e);
        }

        public void SelectDefaultKey()
        {
            cmbo.SelectDefaultKey();
        }

        public event EventHandler SelectedIndexChanged;
    }
}

using GitCommands.Gpg;

namespace GitUI.UserControls.GPGKeys
{
    public class ToolStripGPGSecretKeysSelector : ToolStripControlHost, IDisposable
    {
        private GpgSecretKeysSelector cmbo { get => (GpgSecretKeysSelector)Control; }
        #region Bubble Events

        private void cmbo_SelectedIndexChnaged(object sender, EventArgs e)
        {
            OnSelectedIndexChanged(this, EventArgs.Empty);
        }

        private void cmbo_KeysLoaded(object sender, EventArgs e)
        {
            OnKeysLoaded(EventArgs.Empty);
        }

        #endregion
        public ToolStripGPGSecretKeysSelector() : base(new GpgSecretKeysSelector())
        {
            AutoSize = false;
            AutoToolTip = false; // Use TooltipText for tooltip
            cmbo.Size = Size;
            cmbo.Dock = DockStyle.Fill;
            cmbo.SelectedIndexChanged += new EventHandler(cmbo_SelectedIndexChnaged);
            cmbo.KeysLoaded += new EventHandler(cmbo_KeysLoaded);
        }

        public string KeyID { get => cmbo.KeyID; set => cmbo.KeyID = value; }
        public bool IsDefaultSelected => cmbo.IsDefaultSelected;
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
        protected virtual void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedIndexChanged?.Invoke(sender, e);
        }

        protected virtual void OnKeysLoaded(EventArgs e)
        {
            KeysLoaded?.Invoke(this, e);
        }

        public void SelectDefaultKey()
        {
            cmbo.SelectDefaultKey();
        }

        public void Initialize(IGitUICommandsSource source)
        {
            cmbo.Initilize(source, new GpgSecretKeysParser());
        }

        public event EventHandler SelectedIndexChanged;
        public event EventHandler KeysLoaded;
        protected override bool DismissWhenClicked => false;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cmbo.SelectedIndexChanged -= new EventHandler(cmbo_SelectedIndexChnaged);
                cmbo.KeysLoaded -= new EventHandler(cmbo_KeysLoaded);
                cmbo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

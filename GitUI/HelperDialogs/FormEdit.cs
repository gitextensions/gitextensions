namespace GitUI.HelperDialogs
{
    public partial class FormEdit : GitModuleForm
    {
        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormEdit()
        {
            InitializeComponent();
        }

        public FormEdit(GitUICommands commands, string text)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
            Viewer.InvokeAndForget(() => Viewer.ViewTextAsync("", text));
            Viewer.IsReadOnly = false;
        }

        public bool IsReadOnly
        {
            get => Viewer.IsReadOnly;
            set => Viewer.IsReadOnly = value;
        }
    }
}

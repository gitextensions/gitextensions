namespace GitUI.HelperDialogs
{
    public partial class FormEdit : GitModuleForm
    {
        /// <summary>
        /// For VS designer and translation test.
        /// </summary>
        private FormEdit()
        {
            InitializeComponent();
        }

        public FormEdit(GitUICommands commands, string text)
            : base(true, commands)
        {
            InitializeComponent();
            InitializeComplete();
            ThreadHelper.JoinableTaskFactory.RunAsync(
                () => Viewer.ViewTextAsync("", text));
            Viewer.IsReadOnly = false;
        }

        public bool IsReadOnly
        {
            get => Viewer.IsReadOnly;
            set => Viewer.IsReadOnly = value;
        }
    }
}
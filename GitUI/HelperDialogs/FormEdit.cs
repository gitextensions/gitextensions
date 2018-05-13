namespace GitUI.HelperDialogs
{
    public partial class FormEdit : GitModuleForm
    {
        public FormEdit(GitUICommands commands, string text)
            : base(true, commands)
        {
            InitializeComponent();
            Translate();
            Viewer.ViewTextAsync("", text);
            Viewer.IsReadOnly = false;
        }

        public bool IsReadOnly
        {
            get { return Viewer.IsReadOnly; }
            set { Viewer.IsReadOnly = value; }
        }
    }
}
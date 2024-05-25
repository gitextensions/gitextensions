using GitExtensions.Extensibility.Git;

namespace GitUI.HelperDialogs
{
    public partial class FormEdit : GitModuleForm
    {
        public FormEdit(IGitUICommands commands, string text)
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

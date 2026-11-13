using GitExtensions.Extensibility.Git;
using GitUI.Editor;

namespace GitUI.HelperDialogs;

public partial class FormEdit : GitModuleForm
{
    public FormEdit()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormEdit(IGitUICommands commands, string text, string filename = "")
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();
        Viewer.UICommandsSource = this;
        InitializeComplete();
        Viewer.ViewText(filename, text);
        Viewer.IsReadOnly = false;
    }

    public bool IsReadOnly
    {
        get => Viewer.IsReadOnly;
        set => Viewer.IsReadOnly = value;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormEdit form)
    {
        public FileViewer Viewer => form.Viewer;

        public void LoadText(string filename, string text)
        {
            form.Viewer.ViewText(filename, text);
            form.Viewer.IsReadOnly = false;
        }
    }
}

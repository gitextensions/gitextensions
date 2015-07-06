using ResourceManager;

namespace GitUI.HelperDialogs
{
    public partial class FormEdit : GitExtensionsForm
    {
        public FormEdit(string text)
            : base(true)
        {
            InitializeComponent();
            Translate();
            Viewer.ViewText("", text);
            Viewer.IsReadOnly = false;
        }
    }
}
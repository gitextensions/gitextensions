namespace GitUI.HelperDialogs
{
    public partial class FormEdit : GitExtensionsForm
    {
        public FormEdit(string text)
            : base(true)
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
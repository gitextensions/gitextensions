using Avalonia.Controls;

namespace GitUI.UserControls;

public sealed partial class RemotesComboboxControl : GitModuleControl
{
    public RemotesComboboxControl()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => LoadRemotes();
        InitializeComplete();
        AllowMultiselect = false;
    }

    private bool _remotesLoaded;

    public string SelectedRemote
    {
        get => comboBoxRemotes.Text ?? string.Empty;
        set
        {
            comboBoxRemotes.SelectedItem = comboBoxRemotes.Items
                .OfType<string>()
                .FirstOrDefault(remote => StringComparer.OrdinalIgnoreCase.Equals(remote, value));
            comboBoxRemotes.Text = value;
        }
    }

    private bool _allowMultiselect;

    public bool AllowMultiselect
    {
        get => _allowMultiselect;
        set
        {
            _allowMultiselect = value;
            buttonSelectMultipleRemotes.IsVisible = value;
            if (value)
            {
                throw new NotImplementedException();
            }
        }
    }

    private void LoadRemotes()
    {
        if (_remotesLoaded || Design.IsDesignMode)
        {
            return;
        }

        comboBoxRemotes.ItemsSource = Module.GetRemoteNames();
        _remotesLoaded = true;
    }
}

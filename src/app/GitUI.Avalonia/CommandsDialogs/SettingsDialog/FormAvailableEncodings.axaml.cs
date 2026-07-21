using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GitCommands;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog;

public sealed partial class FormAvailableEncodings : GitExtensionsFormBase
{
    private readonly ObservableCollection<Encoding> _availableEncodings = [];
    private readonly ObservableCollection<Encoding> _includedEncodings = [];

    public FormAvailableEncodings()
    {
        InitializeComponent();
        AcceptButton = ButtonOk;
        ConfigureLists();
        WireEvents();
        InitializeComplete();
        LoadEncoding();
    }

    private void ConfigureLists()
    {
        FuncDataTemplate<Encoding> template = new(
            (encoding, _) => new TextBlock { Text = encoding?.EncodingName },
            supportsRecycling: true);
        ListIncludedEncodings.ItemTemplate = template;
        ListAvailableEncodings.ItemTemplate = template;
        ListIncludedEncodings.ItemsSource = _includedEncodings;
        ListAvailableEncodings.ItemsSource = _availableEncodings;
    }

    private void WireEvents()
    {
        ToRight.Click += ToRight_Click;
        ToLeft.Click += ToLeft_Click;
        ButtonOk.Click += ButtonOk_Click;
        ButtonCancel.Click += ButtonCancel_Click;
        ListIncludedEncodings.SelectionChanged += (_, _) => UpdateActions();
        ListAvailableEncodings.SelectionChanged += (_, _) => UpdateActions();
    }

    private void LoadEncoding()
    {
        foreach (Encoding encoding in AppSettings.AvailableEncodings.Values)
        {
            _includedEncodings.Add(encoding);
        }

        IEnumerable<Encoding> selectableEncodings = Encoding.GetEncodings()
            .Select(info => info.GetEncoding())
            .Select(encoding => encoding.GetType() == typeof(UTF8Encoding)
                ? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                : encoding)
#pragma warning disable SYSLIB0001
            .Where(encoding => encoding != Encoding.UTF7)
#pragma warning restore SYSLIB0001
            .Where(encoding => !AppSettings.AvailableEncodings.ContainsKey(encoding.WebName))
            .GroupBy(encoding => encoding.WebName)
            .Select(group => group.First())
            .OrderBy(encoding => encoding.EncodingName, StringComparer.CurrentCultureIgnoreCase);
        foreach (Encoding encoding in selectableEncodings)
        {
            _availableEncodings.Add(encoding);
        }

        UpdateActions();
    }

    private void ToLeft_Click(object? sender, EventArgs e)
    {
        if (ListAvailableEncodings.SelectedItem is not Encoding encoding)
        {
            return;
        }

        _availableEncodings.Remove(encoding);
        _includedEncodings.Add(encoding);
        ListIncludedEncodings.SelectedItem = encoding;
    }

    private void ToRight_Click(object? sender, EventArgs e)
    {
        if (ListIncludedEncodings.SelectedItem is not Encoding encoding || IsRequiredEncoding(encoding))
        {
            return;
        }

        _includedEncodings.Remove(encoding);
        InsertAvailableEncoding(encoding);
        ListAvailableEncodings.SelectedItem = encoding;
    }

    private void ButtonOk_Click(object? sender, EventArgs e)
    {
        AppSettings.AvailableEncodings.Clear();
        foreach (Encoding encoding in _includedEncodings)
        {
            AppSettings.AvailableEncodings[encoding.WebName] = encoding;
        }

        DialogResult = WinFormsShims.DialogResult.OK;
        Close();
    }

    private void ButtonCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = WinFormsShims.DialogResult.Cancel;
        Close();
    }

    private void UpdateActions()
    {
        ToLeft.IsEnabled = ListAvailableEncodings.SelectedItem is Encoding;
        ToRight.IsEnabled = ListIncludedEncodings.SelectedItem is Encoding encoding && !IsRequiredEncoding(encoding);
    }

    private void InsertAvailableEncoding(Encoding encoding)
    {
        int index = 0;
        while (index < _availableEncodings.Count
               && StringComparer.CurrentCultureIgnoreCase.Compare(_availableEncodings[index].EncodingName, encoding.EncodingName) < 0)
        {
            index++;
        }

        _availableEncodings.Insert(index, encoding);
    }

    private static bool IsRequiredEncoding(Encoding encoding)
    {
        Type encodingType = encoding.GetType();
        return encodingType == typeof(ASCIIEncoding)
            || encodingType == typeof(UnicodeEncoding)
            || encodingType == typeof(UTF8Encoding)
#pragma warning disable SYSLIB0001
            || encodingType == typeof(UTF7Encoding)
#pragma warning restore SYSLIB0001
            || encoding == Encoding.Default;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormAvailableEncodings form)
    {
        public IReadOnlyList<Encoding> Included => form._includedEncodings;

        public IReadOnlyList<Encoding> Available => form._availableEncodings;

        public Button Add => form.ToLeft;

        public Button Remove => form.ToRight;
    }
}

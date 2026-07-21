using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

public partial class FontDialogWindow : GitExtensionsFormBase
{
    public FontDialogWindow()
        : this(new WinFormsShims.Font(FontManager.Current.DefaultFontFamily.Name, 9), fixedPitchOnly: false)
    {
    }

    internal FontDialogWindow(WinFormsShims.Font font, bool fixedPitchOnly)
    {
        InitializeComponent();
        btnOk.Content = TranslatedStrings.OK;
        btnCancel.Content = TranslatedStrings.Cancel;
        AcceptButton = btnOk;
        SelectedFont = font;
        PopulateFontFamilies(font, fixedPitchOnly);
        nudFontSize.Value = (decimal)font.Size;
        chkBold.IsChecked = font.Bold;
        chkItalic.IsChecked = font.Italic;
        WireEvents();
        UpdatePreview();
    }

    internal WinFormsShims.Font SelectedFont { get; private set; }

    private void PopulateFontFamilies(WinFormsShims.Font font, bool fixedPitchOnly)
    {
        List<string> fontNames = FontDialog.GetSystemFontNames(fixedPitchOnly).ToList();
        string? installedName = fontNames.FirstOrDefault(
            name => string.Equals(name, font.Name, StringComparison.CurrentCultureIgnoreCase));
        if (installedName is null)
        {
            fontNames.Insert(0, font.Name);
        }

        cbxFontFamily.ItemsSource = fontNames;
        cbxFontFamily.SelectedItem = installedName ?? font.Name;
        cbxFontFamily.Text = installedName ?? font.Name;
    }

    private void WireEvents()
    {
        cbxFontFamily.SelectionChanged += (_, _) => UpdatePreview();
        cbxFontFamily.PropertyChanged += (_, e) =>
        {
            if (e.Property == ComboBox.TextProperty)
            {
                UpdatePreview();
            }
        };
        nudFontSize.ValueChanged += (_, _) => UpdatePreview();
        chkBold.IsCheckedChanged += (_, _) => UpdatePreview();
        chkItalic.IsCheckedChanged += (_, _) => UpdatePreview();
        btnOk.Click += (_, _) => AcceptSelection();
        btnCancel.Click += (_, _) => DialogResult = WinFormsShims.DialogResult.Cancel;
    }

    private void AcceptSelection()
    {
        SelectedFont = CreateSelectedFont();
        DialogResult = WinFormsShims.DialogResult.OK;
    }

    private WinFormsShims.Font CreateSelectedFont()
    {
        string? familyName = cbxFontFamily.Text?.Trim();
        if (string.IsNullOrEmpty(familyName))
        {
            familyName = FontManager.Current.DefaultFontFamily.Name;
        }

        float size = (float)(nudFontSize.Value ?? 9);
        WinFormsShims.FontStyle style = WinFormsShims.FontStyle.Regular;
        if (chkBold.IsChecked == true)
        {
            style |= WinFormsShims.FontStyle.Bold;
        }

        if (chkItalic.IsChecked == true)
        {
            style |= WinFormsShims.FontStyle.Italic;
        }

        return new WinFormsShims.Font(familyName, size, style);
    }

    private void UpdatePreview()
    {
        WinFormsShims.Font font = CreateSelectedFont();
        txtPreview.FontFamily = new FontFamily(font.Name);
        txtPreview.FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(font.Size);
        txtPreview.FontStyle = font.Italic ? FontStyle.Italic : FontStyle.Normal;
        txtPreview.FontWeight = font.Bold ? FontWeight.Bold : FontWeight.Normal;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FontDialogWindow dialog)
    {
        public ComboBox FontFamily => dialog.cbxFontFamily;

        public NumericUpDown FontSize => dialog.nudFontSize;

        public ToggleButton Bold => dialog.chkBold;

        public ToggleButton Italic => dialog.chkItalic;

        public TextBlock Preview => dialog.txtPreview;

        public void Accept() => dialog.AcceptSelection();
    }
}

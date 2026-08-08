using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using GitCommands;
using GitExtensions.Extensibility.Translations;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class FormChooseTranslation : GitExtensionsForm
{
    private readonly List<Bitmap> _translationImages = [];
    private bool _translationsLoaded;

    public FormChooseTranslation()
    {
        InitializeComponent();
        Text = "Choose language";
        lvTranslations.SelectionChanged += lvTranslations_ItemActivate;
        lvTranslations.KeyDown += lvTranslations_KeyDown;
        InitializeComplete();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        LoadTranslations();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        FormChooseTranslation_FormClosing(this, e);
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        foreach (Bitmap image in _translationImages)
        {
            image.Dispose();
        }

        base.OnClosed(e);
    }

    private void LoadTranslations()
    {
        if (_translationsLoaded)
        {
            return;
        }

        _translationsLoaded = true;
        List<string> translations = [.. Translator.GetAllTranslations()];
        translations.Sort();
        translations.Insert(0, "English");

        List<ListBoxItem> items = [];
        foreach (string translation in translations)
        {
            StackPanel content = new()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                VerticalAlignment = VerticalAlignment.Center,
            };
            string imagePath = Path.Join(Translator.GetTranslationDir(), translation + ".gif");
            if (File.Exists(imagePath))
            {
                Bitmap image = new(imagePath);
                _translationImages.Add(image);
                content.Children.Add(new Image
                {
                    Width = 150,
                    Height = 75,
                    Source = image,
                });
            }

            content.Children.Add(new TextBlock
            {
                Text = translation,
                VerticalAlignment = VerticalAlignment.Center,
            });
            items.Add(new ListBoxItem
            {
                Content = content,
                Tag = translation,
            });
        }

        lvTranslations.ItemsSource = items;
    }

    private void FormChooseTranslation_FormClosing(object? sender, WindowClosingEventArgs e)
    {
        if (string.IsNullOrEmpty(AppSettings.Translation))
        {
            AppSettings.Translation = "English";
        }
    }

    private void lvTranslations_ItemActivate(object? sender, SelectionChangedEventArgs e)
    {
        // take the selection if any, else see the fallback in FormChooseTranslation_FormClosing
        ActivateSelectedTranslation();
    }

    private void ActivateSelectedTranslation()
    {
        if (lvTranslations.SelectedItem is ListBoxItem { Tag: string translation })
        {
            AppSettings.Translation = translation;
            Close();
        }
    }

    private void lvTranslations_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ActivateSelectedTranslation();
            e.Handled = true;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormChooseTranslation form)
    {
        internal ListBox Translations => form.lvTranslations;
        internal void LoadTranslations() => form.LoadTranslations();
    }
}

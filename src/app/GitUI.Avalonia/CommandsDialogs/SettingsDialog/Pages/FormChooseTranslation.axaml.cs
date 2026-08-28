using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
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
        lvTranslations.KeyDown += lvTranslations_KeyDown;
        InitializeComplete();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
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
            Grid content = new()
            {
                Width = 190,
                Height = 98,
                RowDefinitions = new RowDefinitions("78,20"),
            };
            Border imageHost = new()
            {
                Width = 150,
                Height = 75,
                Margin = new Avalonia.Thickness(0, 3, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
            };
            string imagePath = Path.Join(Translator.GetTranslationDir(), translation + ".gif");
            if (File.Exists(imagePath))
            {
                Bitmap image = new(imagePath);
                _translationImages.Add(image);
                imageHost.Child = new Image
                {
                    Width = 150,
                    Height = 75,
                    Stretch = Stretch.None,
                    Source = image,
                };
            }

            content.Children.Add(imageHost);
            TextBlock label = new()
            {
                Text = translation,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetRow(label, 1);
            content.Children.Add(label);

            ListBoxItem item = new()
            {
                Content = content,
                Tag = translation,
            };
            item.PointerEntered += lvTranslations_ItemPointerEntered;
            item.PointerReleased += lvTranslations_ItemActivate;
            items.Add(item);
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

    private void lvTranslations_ItemActivate(object? sender, PointerReleasedEventArgs e)
    {
        // take the selection if any, else see the fallback in FormChooseTranslation_FormClosing
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            ActivateSelectedTranslation();
            e.Handled = true;
        }
    }

    private void lvTranslations_ItemPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is ListBoxItem item)
        {
            // Avalonia constraint: reproduce NativeListView.HoverSelection at the item boundary.
            lvTranslations.SelectedItem = item;
        }
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
        internal void ActivateSelectedTranslation() => form.ActivateSelectedTranslation();
    }
}

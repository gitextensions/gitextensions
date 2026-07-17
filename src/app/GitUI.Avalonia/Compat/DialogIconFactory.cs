using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GitUI.Properties;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

internal static class DialogIconFactory
{
    public static Control? Create(WinFormsShims.MessageBoxIcon icon)
        => icon switch
        {
            WinFormsShims.MessageBoxIcon.Error => CreateImage(Images.StatusBadgeError),
            WinFormsShims.MessageBoxIcon.Question => CreateQuestion(),
            WinFormsShims.MessageBoxIcon.Warning => CreateImage(Images.Warning),
            WinFormsShims.MessageBoxIcon.Information => CreateImage(Images.Information),
            _ => null,
        };

    public static Control? Create(TaskDialogIcon icon)
        => icon switch
        {
            TaskDialogIcon.Information => CreateImage(Images.Information),
            TaskDialogIcon.Warning => CreateImage(Images.Warning),
            TaskDialogIcon.Error => CreateImage(Images.StatusBadgeError),
            _ => null,
        };

    private static Image CreateImage(Avalonia.Media.Imaging.Bitmap bitmap)
        => new()
        {
            Source = bitmap,
            Width = 32,
            Height = 32,
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Top,
        };

    private static Border CreateQuestion()
        => new()
        {
            Width = 32,
            Height = 32,
            CornerRadius = new CornerRadius(16),
            Background = Brushes.RoyalBlue,
            VerticalAlignment = VerticalAlignment.Top,
            Child = new TextBlock
            {
                Text = "?",
                Foreground = Brushes.White,
                FontSize = 22,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
}

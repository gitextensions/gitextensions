using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Layout;
using GitUI.Compat;
using ResourceManager;

namespace GitUI.CommandsDialogs.AboutBoxDialog;

// The original has no XLF catalog entry; its contributor strings come from the packaged resources
// and are shown as-is, so the twin contributes no translation keys.
[Untranslated]
public sealed partial class FormContributors : GitExtensionsForm
{
    private readonly TranslationString _developers = new("Developers");
    private readonly TranslationString _translators = new("Translators");
    private readonly TranslationString _designers = new("Designers");
    private readonly TranslationString _team = new("Team");
    private readonly TranslationString _contributors = new("Contributors");
    private readonly TranslationString _caption = new("The application would not be possible without...");

    [GeneratedRegex(@"\r\n?|\n", RegexOptions.ExplicitCapture)]
    private static partial Regex NewlineRegex { get; }

    // A code-built Window subclass must adopt the framework Window theme.
    protected override Type StyleKeyOverride => typeof(Window);

    public FormContributors()
    {
        InitialiseComponent();
        InitializeComplete();

        void InitialiseComponent()
        {
            TabControl tabControl = GetNewTabControl();

            string[] tabCaptions = [_developers.Text, _translators.Text, _designers.Text];
            TextBox[] textBoxes = new TextBox[tabCaptions.Length];
            for (int i = 0; i < tabCaptions.Length; i++)
            {
                textBoxes[i] = GetNewTextBox();
                GetNewTabPage(textBoxes[i], tabCaptions[i]);
            }

            textBoxes[0].Text = string.Format("{0}:\r\n{1}\r\n\r\n{2}:\r\n{3}",
                _team.Text, NewlineRegex.Replace(Properties.Resources.Team, " "),
                _contributors.Text, NewlineRegex.Replace(Properties.Resources.Coders, " "));
            textBoxes[1].Text = NewlineRegex.Replace(Properties.Resources.Translators, " ");
            textBoxes[2].Text = NewlineRegex.Replace(Properties.Resources.Designers, " ");

            Content = tabControl;

            Width = 624;
            Height = 442;
            CanResize = false;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Text = _caption.Text;

            return;

            TextBox GetNewTextBox()
            {
                return new TextBox
                {
                    BorderThickness = new Avalonia.Thickness(0),
                    Margin = new Avalonia.Thickness(0),
                    AcceptsReturn = true,
                    IsReadOnly = true,
                    TextWrapping = Avalonia.Media.TextWrapping.NoWrap,
                };
            }

            TabItem GetNewTabPage(TextBox textBox, string caption)
            {
                TabItem tabPage = new()
                {
                    Margin = new Avalonia.Thickness(0),
                    Padding = new Avalonia.Thickness(0),
                    Header = caption,
                    Content = textBox,
                };
                tabControl.Items.Add(tabPage);
                return tabPage;
            }

            TabControl GetNewTabControl()
            {
                return new FullBleedTabControl
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    SelectedIndex = 0,
                };
            }
        }
    }
}

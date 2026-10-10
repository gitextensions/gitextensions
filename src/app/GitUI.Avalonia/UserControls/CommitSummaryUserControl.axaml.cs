using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUIPluginInterfaces;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace GitUI.UserControls;

/// <summary>
/// TODO: replace with some better looking RTF control (similar to Commit Tab in main window)
///       Avalonia replaces the RichTextBox experiment with native TextBlocks because the
///       WinForms control displayed plain text without formatting.
/// </summary>
public partial class CommitSummaryUserControl : GitExtensionsControl
{
    private const int MaxBranchTagLength = 75;
    private readonly TranslationString _noRevision = new("No revision");
    private readonly TranslationString _notAvailable = new("n/a");
    private readonly IDateFormatter _dateFormatter = new DateFormatter();
    private readonly string _tagsCaption;
    private readonly string _branchesCaption;
    private GitRevision? _revision;

    public CommitSummaryUserControl()
    {
        InitializeComponent();
        InitializeComplete();
        _tagsCaption = labelTagsCaption.Text ?? string.Empty;
        _branchesCaption = labelBranchesCaption.Text ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets a revision for which to show a summary.
    /// </summary>
    public GitRevision? Revision
    {
        get => _revision;
        set
        {
            _revision = value;

            labelAuthorCaption.Text = ResourceManager.TranslatedStrings.Author + ":";
            labelDateCaption.Text = ResourceManager.TranslatedStrings.CommitDate + ":";
            labelTagsCaption.Text = _tagsCaption;
            labelBranchesCaption.Text = _branchesCaption;

            if (Revision is not null)
            {
                groupBox1.Header = Revision.ObjectId.ToShortString();
                labelAuthor.Text = Revision.Author;
                labelDate.Text = _dateFormatter.FormatDateAsRelativeLocal(Revision.CommitDate);
                labelMessage.Text = Revision.Subject;

                List<IGitRef> tagList = [.. Revision.Refs.Where(r => r.IsTag)];
                if (tagList.Count != 0)
                {
                    SetRefAppearance(
                        labelTags,
                        "GitExtensionsCommitSummaryTagsBackgroundBrush",
                        "GitExtensionsCommitSummaryTagsForegroundBrush");
                    string tagListStr = string.Join(", ", tagList.Select(h => h.LocalName)).ShortenTo(MaxBranchTagLength);
                    labelTags.Text = tagListStr;
                }
                else
                {
                    labelTags.Text = _notAvailable.Text;
                }

                List<IGitRef> branchesList = [.. Revision.Refs.Where(r => r.IsHead)];
                if (branchesList.Count != 0)
                {
                    SetRefAppearance(
                        labelBranches,
                        "GitExtensionsCommitSummaryBranchesBackgroundBrush",
                        "GitExtensionsCommitSummaryBranchesForegroundBrush");
                    string branchesListStr = string.Join(", ", branchesList.Select(h => h.LocalName)).ShortenTo(MaxBranchTagLength);
                    labelBranches.Text = branchesListStr;
                }
                else
                {
                    labelBranches.Text = _notAvailable.Text;
                }
            }
            else
            {
                groupBox1.Header = _noRevision.Text;
                labelAuthor.Text = "---";
                labelDate.Text = "---";
                labelMessage.Text = "---";
                labelTags.Text = "---";
                labelTags.ClearValue(TextBlock.BackgroundProperty);
                labelBranches.Text = "---";
                labelBranches.ClearValue(TextBlock.BackgroundProperty);
            }
        }
    }

    // Avalonia constraint: dynamic resources preserve the original adapted label colors when
    // the application switches between light, dark, and custom themes at runtime.
    private static void SetRefAppearance(TextBlock label, string backgroundResourceKey, string foregroundResourceKey)
    {
        label[!TextBlock.BackgroundProperty] = new DynamicResourceExtension(backgroundResourceKey);
        label[!TextBlock.ForegroundProperty] = new DynamicResourceExtension(foregroundResourceKey);
        label.FontWeight = FontWeight.Bold;
    }
}

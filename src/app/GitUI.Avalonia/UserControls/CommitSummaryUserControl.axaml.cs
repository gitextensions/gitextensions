using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUIPluginInterfaces;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace GitUI.UserControls;

/// <summary>Displays the identifying details of a revision.</summary>
public partial class CommitSummaryUserControl : GitExtensionsControl
{
    private const int MaxBranchTagLength = 75;
    private readonly TranslationString _noRevision = new("No revision");
    private readonly TranslationString _notAvailable = new("n/a");
    private readonly IDateFormatter _dateFormatter = new DateFormatter();
    private GitRevision? _revision;

    public CommitSummaryUserControl()
    {
        InitializeComponent();
        InitializeComplete();
        Revision = null;
    }

    /// <summary>Gets or sets the revision for which to show a summary.</summary>
    public GitRevision? Revision
    {
        get => _revision;
        set
        {
            _revision = value;
            if (value is null)
            {
                groupBox1.Header = _noRevision.Text;
                labelAuthor.Text = "---";
                labelDate.Text = "---";
                labelMessage.Text = "---";
                labelTags.Text = "---";
                labelBranches.Text = "---";
                return;
            }

            groupBox1.Header = value.ObjectId.ToShortString();
            labelAuthor.Text = value.Author;
            labelDate.Text = _dateFormatter.FormatDateAsRelativeLocal(value.CommitDate);
            labelMessage.Text = value.Subject;
            labelTags.Text = FormatRefs(value.Refs.Where(gitRef => gitRef.IsTag));
            labelBranches.Text = FormatRefs(value.Refs.Where(gitRef => gitRef.IsHead));
        }
    }

    private string FormatRefs(IEnumerable<IGitRef> refs)
    {
        string text = string.Join(", ", refs.Select(gitRef => gitRef.LocalName));
        return string.IsNullOrEmpty(text) ? _notAvailable.Text : text.ShortenTo(MaxBranchTagLength);
    }
}

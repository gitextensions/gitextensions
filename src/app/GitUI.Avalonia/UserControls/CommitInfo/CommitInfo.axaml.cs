using Avalonia.Controls;
using GitUIPluginInterfaces;

namespace GitUI.CommitInfo;

// TODO(avalonia-port): milestone M1.4 — a plain commit details pane. The WinForms CommitInfo's
// links, tags/branches resolution, and rendered header arrive in later milestones.
public partial class CommitInfo : UserControl
{
    private GitRevision? _revision;

    public CommitInfo()
    {
        InitializeComponent();
    }

    /// <summary>
    ///  Gets or sets the revision whose details are shown (named like the WinForms property).
    /// </summary>
    public GitRevision? Revision
    {
        get => _revision;
        set
        {
            _revision = value;

            if (value is null)
            {
                RevisionHeader.Text = string.Empty;
                rtbxCommitMessage.Text = string.Empty;
                return;
            }

            string refs = value.Refs.Count > 0
                ? $"\nRefs:      {string.Join(", ", value.Refs.Select(gitRef => gitRef.Name))}"
                : string.Empty;
            RevisionHeader.Text =
                $"Commit:    {value.ObjectId}\n" +
                $"Author:    {value.Author} <{value.AuthorEmail}>\n" +
                $"Date:      {value.AuthorDate:yyyy-MM-dd HH:mm:ss}{refs}";
            rtbxCommitMessage.Text = string.IsNullOrEmpty(value.Body) ? value.Subject : value.Body;
        }
    }
}

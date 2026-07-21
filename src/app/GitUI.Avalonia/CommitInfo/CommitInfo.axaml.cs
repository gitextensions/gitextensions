using Avalonia.Controls;
using GitCommands;
using GitUIPluginInterfaces;

using ResourceManager;

namespace GitUI.CommitInfo;

public partial class CommitInfo : GitExtensionsControl
{
    private GitRevision? _revision;

    public CommitInfo()
    {
        InitializeComponent();

        InitializeComplete();
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
                avatarControl.IsVisible = false;
                avatarControl.LoadImage(null, null);
                RevisionHeader.Text = string.Empty;
                rtbxCommitMessage.Text = string.Empty;
                return;
            }

            avatarControl.IsVisible = AppSettings.ShowAuthorAvatarInCommitInfo;
            avatarControl.LoadImage(value.AuthorEmail ?? value.CommitterEmail, value.Author ?? value.Committer);

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

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(CommitInfo commitInfo)
    {
        public AvatarControl Avatar => commitInfo.avatarControl;
    }
}

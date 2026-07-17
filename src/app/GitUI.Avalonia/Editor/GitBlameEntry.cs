namespace GitUI.Editor;

// Twin of GitUI/Editor/GitBlameEntry.cs without the Avatar image; avatars arrive with
// the avatar subphase.
public class GitBlameEntry
{
    public int AgeBucketIndex { get; set; }
    public Color AgeBucketColor { get; set; }
}

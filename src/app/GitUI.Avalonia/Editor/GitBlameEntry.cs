namespace GitUI.Editor;

// Twin of GitUI/Editor/GitBlameEntry.cs without the Avatar image. Loading avatars without
// blocking blame parsing or making the author margin own disposable bitmaps remains separate.
public class GitBlameEntry
{
    public int AgeBucketIndex { get; set; }
    public Color AgeBucketColor { get; set; }
}

using System.Drawing;

namespace GitUI.Editor
{
    public class GitBlameEntry
    {
        public Image Avatar { get; set; }
        public int AgeBucketIndex { get; set; }
        public Color AgeBucketColor { get; set; }
    }
}

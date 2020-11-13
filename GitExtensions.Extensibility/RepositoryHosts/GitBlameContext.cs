using GitExtensions.Core.Module;

namespace GitExtensions.Extensibility.RepositoryHosts
{
    // This is stored in ContextMenuStrip.Tag, so converting to a struct won't be benefitial
    public class GitBlameContext
    {
        public GitBlameContext(string fileName, int lineIndex, int blameLine, ObjectId blameId)
        {
            FileName = fileName;
            LineIndex = lineIndex;
            BlameLine = blameLine;
            BlameId = blameId;
        }

        public int LineIndex { get; }
        public int BlameLine { get; }
        public ObjectId BlameId { get; }
        public string FileName { get; }
    }
}

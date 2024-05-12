using GitExtensions.Extensibility.Git;

namespace GitUI
{
    internal sealed class SuperProjectInfo
    {
        public ObjectId? CurrentCommit { get; set; }
        public ObjectId? ConflictBase { get; set; }
        public ObjectId? ConflictRemote { get; set; }
        public ObjectId? ConflictLocal { get; set; }
        public IReadOnlyDictionary<ObjectId, IReadOnlyList<IGitRef>>? Refs { get; set; }
    }
}

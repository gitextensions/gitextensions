using System.Collections.Generic;
using GitUIPluginInterfaces;

namespace GitUI
{
    internal sealed class SuperProjectInfo
    {
        // TODO nothing reads this property
        public ObjectId? CurrentBranch { get; set; }
        public ObjectId? ConflictBase { get; set; }
        public ObjectId? ConflictRemote { get; set; }
        public ObjectId? ConflictLocal { get; set; }
        public IReadOnlyDictionary<ObjectId, IReadOnlyList<IGitRef>>? Refs { get; set; }
    }
}

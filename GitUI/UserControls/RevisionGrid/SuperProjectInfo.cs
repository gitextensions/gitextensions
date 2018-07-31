using System.Collections.Generic;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI
{
    internal sealed class SuperProjectInfo
    {
        // TODO nothing reads this property
        [CanBeNull] public ObjectId CurrentBranch { get; set; }
        [CanBeNull] public ObjectId ConflictBase { get; set; }
        [CanBeNull] public ObjectId ConflictRemote { get; set; }
        [CanBeNull] public ObjectId ConflictLocal { get; set; }
        [CanBeNull] public IReadOnlyDictionary<ObjectId, IReadOnlyList<IGitRef>> Refs { get; set; }
    }
}
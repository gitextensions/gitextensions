using System.Diagnostics;

namespace GitCommands.Git
{
    [DebuggerDisplay("{" + nameof(Filename) + "}")]
    public readonly struct ConflictData
    {
        public ConflictData(
            ConflictedFileData @base,
            ConflictedFileData local,
            ConflictedFileData remote)
        {
            Base = @base;
            Local = local;
            Remote = remote;
        }

        public ConflictedFileData Base { get; }
        public ConflictedFileData Local { get; }
        public ConflictedFileData Remote { get; }

        public string Filename => Local.Filename ?? Base.Filename ?? Remote.Filename;
    }
}
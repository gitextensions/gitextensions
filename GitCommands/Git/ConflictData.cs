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

        public readonly ConflictedFileData Base { get; }
        public readonly ConflictedFileData Local { get; }
        public readonly ConflictedFileData Remote { get; }

        public readonly string Filename => Local.Filename ?? Base.Filename ?? Remote.Filename;
    }
}
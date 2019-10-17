using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public readonly struct ConflictedFileData
    {
        public ConflictedFileData(ObjectId objectId, string filename)
        {
            ObjectId = objectId;
            Filename = filename;
        }

        public readonly ObjectId ObjectId { get; }
        public readonly string Filename { get; }
    }
}
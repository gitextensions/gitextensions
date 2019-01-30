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

        public ObjectId ObjectId { get; }
        public string Filename { get; }
    }
}
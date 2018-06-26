namespace Gravatar
{
    public static class AvatarService
    {
        // TODO get a backup image
        public static IAvatarProvider Default { get; }
            ////= new BackupAvatarProvider(new AvatarMemoryCache(new AvatarPersistentCache(new AvatarDownloader())), GitUI.Properties.Resources.User);
            = new AvatarMemoryCache(new AvatarPersistentCache(new AvatarDownloader()));
    }
}

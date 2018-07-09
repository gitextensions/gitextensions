namespace GitUI.Avatars
{
    public static class AvatarService
    {
        public static IAvatarProvider Default { get; }
            = new BackupAvatarProvider(new AvatarMemoryCache(new AvatarPersistentCache(new AvatarDownloader())), Properties.Resources.User);
    }
}

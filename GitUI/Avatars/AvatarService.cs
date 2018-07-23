using GitUI.Properties;

namespace GitUI.Avatars
{
    public static class AvatarService
    {
        public static IAvatarProvider Default { get; }
            = new BackupAvatarProvider(new AvatarMemoryCache(new AvatarPersistentCache(new AvatarDownloader())), Images.User80);
    }
}

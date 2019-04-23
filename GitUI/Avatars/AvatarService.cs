using GitUI.Properties;

namespace GitUI.Avatars
{
    public static class AvatarService
    {
        private static InitialsAvatarGenerator AvatarGenerator = new InitialsAvatarGenerator();
        public static IAvatarProvider Default { get; }
            = new BackupAvatarProvider(new AvatarMemoryCache(new AvatarPersistentCache(new AvatarDownloader(), AvatarGenerator)),
                Images.User80);
    }
}

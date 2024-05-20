namespace GitCommands
{
    public enum AvatarProvider
    {
        // DO NOT RENAME THESE -- doing so will break user preferences
        Default = 0, // External providers: Gravatar & Github
        Custom, // External provider: user defined
        None, // Only fallback are used: result is always the same
    }
}

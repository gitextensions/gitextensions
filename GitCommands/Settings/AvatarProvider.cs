using System.ComponentModel;

namespace GitCommands
{
    public enum AvatarProvider
    {
        Gravatar = 0,
        [Description("Author initials")]
        AuthorInitials,
    }
}
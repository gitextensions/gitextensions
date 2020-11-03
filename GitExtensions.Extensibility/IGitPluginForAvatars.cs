using GitExtensions.Core.Avatars;

namespace GitExtensions.Extensibility
{
    public interface IGitPluginForAvatars
    {
        IAvatarProvider AvatarProvider { get; set; }
    }
}

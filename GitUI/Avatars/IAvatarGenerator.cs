using System.Drawing;

namespace GitUI.Avatars
{
    public interface IAvatarGenerator
    {
        Image GetAvatarImage(string email, string name, int imageSize);
        Image GenerateAvatarImage(string email, string name, int imageSize);
    }
}
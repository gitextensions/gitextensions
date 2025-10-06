namespace GitUITests.Avatars;

public abstract class AvatarCacheTestBase : AvatarTestBase
{
    [Test]
    public async Task GetAvatarAsync_returns_correct_image()
    {
        ClassicAssert.AreSame(_img1, await _cache.GetAvatarAsync(_email1, _name1, _size));
        ClassicAssert.AreSame(_img2, await _cache.GetAvatarAsync(_email2, _name2, _size));
        ClassicAssert.AreSame(_img3, await _cache.GetAvatarAsync(_email3, _name3, _size));
    }
}

using System;
using System.Threading.Tasks;
using GitUI.Avatars;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.Avatars
{
    [TestFixture]
    public class CacheControlAvatarProviderTests
    {
        [Test]
        public async Task ClearCache_is_passed_to_all_children()
        {
            var cacheCleaner1 = Substitute.For<IAvatarCacheCleaner>();
            var cacheCleaner2 = Substitute.For<IAvatarCacheCleaner>();
            var cacheCleaner3 = Substitute.For<IAvatarCacheCleaner>();

            var cacheClearedEventHandler = Substitute.For<EventHandler>();

            MultiCacheCleaner cacheCleaner = new(cacheCleaner1, cacheCleaner2, cacheCleaner3);
            cacheCleaner.CacheCleared += cacheClearedEventHandler;

            await cacheCleaner.ClearCacheAsync();

            await cacheCleaner1.Received(1).ClearCacheAsync();
            await cacheCleaner2.Received(1).ClearCacheAsync();
            await cacheCleaner3.Received(1).ClearCacheAsync();

            cacheClearedEventHandler.Received(1)(cacheCleaner, EventArgs.Empty);
        }

        [Test]
        public void Construction_with_null_parameters_is_permitted()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new MultiCacheCleaner(null);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                new MultiCacheCleaner(null);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                new MultiCacheCleaner(null, null);
            });
        }
    }
}

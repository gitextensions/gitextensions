using AwesomeAssertions;
using NUnit.Framework;

namespace GitExtensions.ParityDiff.Tests;

internal sealed class PixelComparerTests
{
    [Test]
    [Category("P8_6i")]
    public void Compare_should_count_every_primary_pixel()
    {
        PngImage reference = new(2, 1, [32, 64, 96, 255, 32, 64, 96, 255]);
        PngImage candidate = new(2, 1, [200, 64, 96, 255, 32, 64, 96, 255]);

        PixelMetrics metrics = PixelComparer.Compare(reference, candidate, 0);

        metrics.ComparedPixelCount.Should().Be(2);
        metrics.DifferentPixelFraction.Should().Be(0.5);
        metrics.MaximumChannelDelta.Should().Be(168);
    }
}

using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace GitExtensions.ParityDiff.Tests;

internal sealed class PixelComparerTests
{
    [Test]
    [Category("P8_6i")]
    public void Compare_should_exclude_only_occluded_pixels()
    {
        PngImage reference = new(2, 1, [32, 64, 96, 255, 32, 64, 96, 255]);
        PngImage candidate = new(2, 1, [200, 64, 96, 255, 32, 64, 96, 255]);

        PixelMetrics unmasked = PixelComparer.Compare(reference, candidate, 0);
        PixelMetrics masked = PixelComparer.Compare(
            reference,
            candidate,
            0,
            [new CaptureRectangle { X = 0, Y = 0, Width = 1, Height = 1 }]);

        unmasked.DifferentPixelFraction.Should().Be(0.5);
        masked.ComparedPixelCount.Should().Be(1);
        masked.DifferentPixelFraction.Should().Be(0);
        masked.MaximumChannelDelta.Should().Be(0);
        masked.Ssim.Should().Be(1);
    }
}

using GitExtUtils.GitUI.Theming;
using GitUI;
using NUnit.Framework;

namespace GitExtUtilsTests
{
    [TestFixture]
    public class ColorTransformationTests
    {
        [Test, Combinatorial]
        public void Within_original_segment_makes_linear_interpolation(
            [Values(0d, 0.5d, 1d)] double exampleOrig,
            [Values(0.25d, 0.75d)] double oppositeOrig,
            [Values(0d, 0.5d, 1d)] double example,
            [Values(0.25d, 0.75d)] double opposite,
            [Values(0d, 0.5d, 1d)] double alpha)
        {
            var transformed = ColorHelper.Transform(
                (exampleOrig * (1 - alpha)) + (oppositeOrig * alpha),
                exampleOrig,
                oppositeOrig,
                example,
                opposite);

            var expected = (example * (1 - alpha)) + (opposite * alpha);
            Assert.That(transformed, Is.EqualTo(expected).Within(0.001));
        }

        [TestCase(0.4d, 0.7d, 0.6d, 0.8d, 0.2d, 0.3d)]
        [TestCase(0.4d, 0.7d, 0.6d, 1.0d, 0.2d, 0.3d)]
        [TestCase(0.4d, 0.7d, 0.6d, 0.0d, 0.2d, 0.8d)]
        [TestCase(0.4d, 0.7d, 0.6d, 0.8d, 0.1d, 0.15d)]
        [TestCase(0.4d, 0.7d, 0.6d, 1.0d, 0.1d, 0.15d)]
        [TestCase(0.4d, 0.7d, 0.6d, 0.0d, 0.1d, 0.9d)]
        [TestCase(0.6d, 0.3d, 0.4d, 0.2d, 0.8d, 0.7d)]
        [TestCase(0.6d, 0.3d, 0.4d, 0.0d, 0.8d, 0.7d)]
        [TestCase(0.6d, 0.3d, 0.4d, 1.0d, 0.8d, 0.2d)]
        [TestCase(0.6d, 0.3d, 0.4d, 0.2d, 0.9d, 0.85d)]
        [TestCase(0.6d, 0.3d, 0.4d, 0.0d, 0.9d, 0.85d)]
        [TestCase(0.6d, 0.3d, 0.4d, 1.0d, 0.9d, 0.1d)]
        public void Outside_original_segment_interpolates_between_boundary_and_segment(
            double exampleOrig, double oppositeOrig, double example, double opposite, double orig, double expected)
        {
            var transformed = ColorHelper.Transform(
                orig,
                exampleOrig,
                oppositeOrig,
                example,
                opposite);

            Assert.That(transformed, Is.EqualTo(expected).Within(0.001));
        }
    }
}

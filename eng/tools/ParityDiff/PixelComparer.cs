namespace GitExtensions.ParityDiff;

// parity-scaffolding: Computes temporary pixel-level parity measurements.
internal static class PixelComparer
{
    private const double SsimC1 = 6.5025;
    private const double SsimC2 = 58.5225;

    public static PixelMetrics Compare(PngImage reference, PngImage candidate, byte channelTolerance)
    {
        int width = Math.Max(reference.Width, candidate.Width);
        int height = Math.Max(reference.Height, candidate.Height);
        int pixelCount = checked(width * height);
        double referenceMean = 0;
        double candidateMean = 0;
        long absoluteChannelDelta = 0;
        int maximumChannelDelta = 0;
        int differentPixels = 0;
        Span<byte> referencePixel = stackalloc byte[4];
        Span<byte> candidatePixel = stackalloc byte[4];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ReadPixel(reference, x, y, referencePixel);
                ReadPixel(candidate, x, y, candidatePixel);
                bool different = false;
                for (int channel = 0; channel < 4; channel++)
                {
                    int delta = Math.Abs(referencePixel[channel] - candidatePixel[channel]);
                    absoluteChannelDelta += delta;
                    maximumChannelDelta = Math.Max(maximumChannelDelta, delta);
                    different |= delta > channelTolerance;
                }

                differentPixels += different ? 1 : 0;
                referenceMean += Luminance(referencePixel);
                candidateMean += Luminance(candidatePixel);
            }
        }

        referenceMean /= pixelCount;
        candidateMean /= pixelCount;
        double referenceVariance = 0;
        double candidateVariance = 0;
        double covariance = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ReadPixel(reference, x, y, referencePixel);
                ReadPixel(candidate, x, y, candidatePixel);
                double referenceDifference = Luminance(referencePixel) - referenceMean;
                double candidateDifference = Luminance(candidatePixel) - candidateMean;
                referenceVariance += referenceDifference * referenceDifference;
                candidateVariance += candidateDifference * candidateDifference;
                covariance += referenceDifference * candidateDifference;
            }
        }

        int divisor = Math.Max(1, pixelCount - 1);
        referenceVariance /= divisor;
        candidateVariance /= divisor;
        covariance /= divisor;
        double luminance = (2 * referenceMean * candidateMean) + SsimC1;
        double structure = (2 * covariance) + SsimC2;
        double luminanceScale = (referenceMean * referenceMean) + (candidateMean * candidateMean) + SsimC1;
        double structureScale = referenceVariance + candidateVariance + SsimC2;
        double ssim = (luminance * structure) / (luminanceScale * structureScale);

        return new PixelMetrics
        {
            ReferenceWidth = reference.Width,
            ReferenceHeight = reference.Height,
            CandidateWidth = candidate.Width,
            CandidateHeight = candidate.Height,
            Ssim = Math.Round(ssim, 6),
            DifferentPixelFraction = Math.Round((double)differentPixels / pixelCount, 6),
            MaximumChannelDelta = maximumChannelDelta,
            MeanAbsoluteChannelDelta = Math.Round((double)absoluteChannelDelta / (pixelCount * 4), 6)
        };
    }

    private static double Luminance(ReadOnlySpan<byte> pixel)
    {
        double alpha = pixel[3] / 255d;
        return ((0.2126 * pixel[0]) + (0.7152 * pixel[1]) + (0.0722 * pixel[2])) * alpha;
    }

    private static void ReadPixel(PngImage image, int x, int y, Span<byte> target)
    {
        if (x >= image.Width || y >= image.Height)
        {
            target.Clear();
            return;
        }

        image.Rgba.AsSpan(((y * image.Width) + x) * 4, 4).CopyTo(target);
    }
}

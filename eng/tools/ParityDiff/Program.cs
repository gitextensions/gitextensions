namespace GitExtensions.ParityDiff;

// parity-scaffolding: Hosts the temporary capture-comparison toolchain.
internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            DiffOptions options = DiffOptions.Parse(args);
            ParityDiffResult result = ParityDiffRunner.Run(options);
            Console.WriteLine(
                $"Compared {result.Summary.ComparedCaptureCount} capture pairs; "
                + $"wrote {result.Summary.FindingCount} findings to {Path.GetFullPath(options.OutputDirectory)}.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}

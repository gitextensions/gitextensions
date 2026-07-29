namespace GitExtensions.ParityInventory;

// parity-scaffolding: Hosts the temporary functional parity inventory tool.
internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            InventoryOptions options = InventoryOptions.Parse(args);
            InventoryReport report = InventoryRunner.Run(options);
            Console.WriteLine(
                $"Compared {report.Original.Parts.Count} original and {report.Twin.Parts.Count} twin parts; "
                + $"wrote {report.Summary.FindingCount} findings to "
                + $"{Path.GetFullPath(options.OutputFile)}.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}

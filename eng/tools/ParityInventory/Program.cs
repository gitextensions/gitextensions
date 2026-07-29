namespace GitExtensions.ParityInventory;

// parity-scaffolding: Hosts the temporary functional parity inventory tool.
internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            if (args.FirstOrDefault()?.Equals("ledger", StringComparison.OrdinalIgnoreCase) == true)
            {
                ParityLedger ledger = LedgerRunner.Run(LedgerOptions.Parse(args));
                Console.WriteLine(
                    $"Wrote {ledger.Components.Count} ledger entries; "
                    + $"{ledger.Components.Count(component => component.Complete)} are complete.");
                return 0;
            }

            if (args.FirstOrDefault()?.Equals("sweep", StringComparison.OrdinalIgnoreCase) == true)
            {
                InventorySweepResult sweep = InventorySweepRunner.Run(SweepOptions.Parse(args));
                Console.WriteLine(
                    $"Assessed {sweep.Summary.MappingCount} mappings and {sweep.Summary.AnalyzedTypeCount} class types; "
                    + $"wrote {sweep.Summary.FindingCount} findings to {Path.GetFullPath(sweep.OutputFile)}.");
                return 0;
            }

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

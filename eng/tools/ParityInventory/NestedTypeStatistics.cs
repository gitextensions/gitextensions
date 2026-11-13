namespace GitExtensions.ParityInventory;

// parity-scaffolding: Counts nested product and test-support declarations without hiding either.
internal static class NestedTypeStatistics
{
    public static int Count(SourceInventory inventory, bool testAccessors) => inventory.Members
        .Where(member => member.Kind is "class" or "struct" or "interface" or "record")
        .Where(member => IsTestAccessor(member) == testAccessors)
        .Select(member => $"{member.ContainingType}/{member.Name}")
        .Distinct(StringComparer.Ordinal).Count();

    private static bool IsTestAccessor(MemberEntry member) => member.Name == "TestAccessor"
        || member.ContainingType?.Split('.').Contains("TestAccessor", StringComparer.Ordinal) == true;
}

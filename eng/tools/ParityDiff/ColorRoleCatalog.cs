using System.Text.Json;

namespace GitExtensions.ParityDiff;

// parity-scaffolding: Defines the temporary framework-neutral color vocabulary used by both capture readers.
internal sealed record ColorRoleCatalog
{
    public const int CurrentSchemaVersion = 1;

    public required int SchemaVersion { get; init; }

    public required IReadOnlyList<ColorRoleDefinition> Roles { get; init; }

    public static ColorRoleCatalog Load(string path)
    {
        ColorRoleCatalog catalog = JsonSerializer.Deserialize<ColorRoleCatalog>(
            File.ReadAllText(path),
            JsonDefaults.ReadOptions)
            ?? throw new InvalidDataException($"Color-role catalog '{path}' is empty.");
        if (catalog.SchemaVersion != CurrentSchemaVersion)
        {
            throw new InvalidDataException($"Unsupported color-role catalog schema version {catalog.SchemaVersion}.");
        }

        if (catalog.Roles.Count == 0)
        {
            throw new InvalidDataException("The color-role catalog must declare at least one semantic role.");
        }

        HashSet<string> identifiers = new(StringComparer.Ordinal);
        foreach (ColorRoleDefinition role in catalog.Roles)
        {
            if (!role.Id.StartsWith("semantic.", StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Color role '{role.Id}' must start with 'semantic.'.");
            }

            if (string.IsNullOrWhiteSpace(role.Meaning))
            {
                throw new InvalidDataException($"Color role '{role.Id}' does not define a framework-neutral meaning.");
            }

            if (!identifiers.Add(role.Id))
            {
                throw new InvalidDataException($"Color role '{role.Id}' is declared more than once.");
            }
        }

        return catalog;
    }
}

// parity-scaffolding: Gives one temporary capture role a framework-neutral meaning.
internal sealed record ColorRoleDefinition
{
    public required string Id { get; init; }

    public required string Meaning { get; init; }
}

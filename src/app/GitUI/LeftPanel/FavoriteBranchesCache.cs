using System.Text.Json;
using System.Text.Json.Serialization;
using GitExtensions.Extensibility.Git;

namespace GitUI.LeftPanel;

internal sealed class FavoriteBranchesCache
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true, Converters = { new ObjectIdConverter() } // Add the custom converter here
    };

    private readonly HashSet<BranchIdentifier> _favorites = new();
    private readonly object _lock = new();
    private bool _isLoaded;
    private string _location = string.Empty;

    internal string ConfigFile
    {
        get => Path.Combine(Location, "GitExtensions.favorite");
    }

    /// <summary>
    /// Gets or sets the location where the favorites configuration file is stored.
    /// </summary>
    internal string Location
    {
        get => _location;
        set
        {
            if (_location != value)
            {
                _location = value;
            }
        }
    }

    /// <summary>
    /// Adds a branch to the favorites list.
    /// </summary>
    /// <param name="objectId">The unique ObjectId of the branch's latest commit.</param>
    /// <param name="branchName">The name of the branch (fully qualified).</param>
    internal void Add(ObjectId objectId, string branchName)
    {
        BranchIdentifier branch = new(objectId, branchName);

        if (!BranchIdentifier.IsValid(branch))
        {
            return;
        }

        lock (_lock)
        {
            if (_favorites.Add(branch))
            {
                Save();
            }
        }
    }

    /// <summary>
    /// Removes a branch from the favorites list.
    /// </summary>
    /// <param name="objectId">The unique ObjectId of the branch's latest commit.</param>
    /// <param name="branchName">The name of the branch (fully qualified).</param>
    public void Remove(ObjectId objectId, string branchName)
    {
        BranchIdentifier branch = new(objectId, branchName);

        if (!BranchIdentifier.IsValid(branch))
        {
            return;
        }

        lock (_lock)
        {
            if (_favorites.Remove(branch))
            {
                Save();
            }
        }
    }

    /// <summary>
    /// Checks if a branch is marked as a favorite.
    /// </summary>
    /// <param name="objectId">The unique ObjectId of the branch's latest commit.</param>
    /// <param name="branchName">The name of the branch (fully qualified).</param>
    /// <returns><c>true</c> if the branch is a favorite; otherwise, <c>false</c>.</returns>
    internal bool Contains(ObjectId objectId, string branchName)
    {
        BranchIdentifier branch = new(objectId, branchName);

        if (!BranchIdentifier.IsValid(branch))
        {
            return false;
        }

        LoadIfNeeded();

        lock (_lock)
        {
            return _favorites.Contains(branch);
        }
    }

    /// <summary>
    /// Cleans up the favorites list by removing branches that no longer exist.
    /// </summary>
    /// <param name="branches">A list of branches to retain in the favorites list.</param>
    internal void CleanUp(IReadOnlyList<IGitRef> branches)
    {
        List<BranchIdentifier> removableList = new();

        lock (_lock)
        {
            foreach (IGitRef? branch in branches)
            {
                if (branch.ObjectId != null)
                {
                    BranchIdentifier item = new(branch.ObjectId, branch.Name);

                    if (!_favorites.Contains(item))
                    {
                        removableList.Add(item);
                    }
                }
            }

            removableList.ForEach(item => _favorites.Remove(item));
            Save();
        }
    }

    /// <summary>
    /// Loads the list of favorite branches from the configuration file into memory.
    /// </summary>
    /// <remarks>
    /// This method reads the configuration file in JSON format and deserializes it into a list of `BranchIdentifier`
    /// objects. If the file does not exist, the method exits without taking any action.
    /// </remarks>
    internal void Load()
    {
        if (!File.Exists(ConfigFile))
        {
            return;
        }

        try
        {
            string json;

            lock (_lock)
            {
                json = File.ReadAllText(ConfigFile);
            }

            BranchIdentifier[]? deserialized = JsonSerializer.Deserialize<BranchIdentifier[]>(json, _jsonOptions);

            if (deserialized is not null)
            {
                lock (_lock)
                {
                    _favorites.UnionWith(deserialized);
                }
            }

            _isLoaded = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load favorites: {ex.Message}");
        }
    }

    private void Save()
    {
        try
        {
            lock (_lock)
            {
                string json = JsonSerializer.Serialize(_favorites, _jsonOptions);
                File.WriteAllText(ConfigFile, json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save favorites: {ex.Message}");
        }
    }

    private void LoadIfNeeded()
    {
        lock (_lock)
        {
            if (!_isLoaded)
            {
                Load();
            }
        }
    }

    private class ObjectIdConverter : JsonConverter<ObjectId>
    {
        public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? idString = reader.GetString();

                return idString is not null
                    ? ObjectId.Parse(idString)
                    : default;
            }

            throw new JsonException($"Unexpected token type {reader.TokenType}, expected a JSON string for ObjectId.");
        }

        public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    internal class BranchIdentifier(ObjectId objectId, string name)
    {
        public ObjectId ObjectId { get; } = objectId;

        public string Name { get; set; } = name;

        public override bool Equals(object obj)
        {
            if (obj is not BranchIdentifier other)
            {
                return false;
            }

            return ObjectId == other.ObjectId || Name == other.Name;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool IsValid(BranchIdentifier branch)
        {
            return branch is not null && branch.ObjectId != default && !string.IsNullOrEmpty(branch.Name);
        }
    }
}

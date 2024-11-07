using System.Diagnostics;
using System.IO.Abstractions;
using System.Text.Json;
using GitExtensions.Extensibility.Git;

namespace GitUI.LeftPanel;

internal sealed class FavoriteBranchesCache
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true, Converters = { new ObjectIdConverter() } };

    private readonly IFileSystem _fileSystem;

    private readonly HashSet<BranchIdentifier> _favorites = new();
    private readonly object _lock = new();
    private bool _isLoaded;
    private string _location = string.Empty;

    public FavoriteBranchesCache(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public FavoriteBranchesCache() : this(new FileSystem())
    {
    }

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
        lock (_lock)
        {
            IEnumerable<BranchIdentifier>? branchIdentifiers = branches
                                                               .Where(branch => branch.ObjectId is not null)
                                                               .Select(branch => new BranchIdentifier(branch.ObjectId, branch.Name));

            _favorites.RemoveWhere(fav => !branchIdentifiers.Contains(fav));

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
        if (!_fileSystem.File.Exists(ConfigFile))
        {
            return;
        }

        lock (_lock)
        {
            TryInvokeIfFileAccessible(ConfigFile,
                () =>
                {
                    try
                    {
                        string json = null;
                        json = _fileSystem.File.ReadAllText(ConfigFile);

                        if (!string.IsNullOrEmpty(json))
                        {
                            BranchIdentifier[]? deserialized = Deserialize<BranchIdentifier[]>(json);

                            if (deserialized is not null)
                            {
                                _favorites.Clear();
                                _favorites.UnionWith(deserialized);
                            }

                            _isLoaded = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Failed to load favorites: {ex.Message}");
                    }
                });
        }
    }

    internal static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    internal static string Serialize<T>(T favorites)
    {
        return JsonSerializer.Serialize(favorites, _jsonOptions);
    }

    internal void Save()
    {
        lock (_lock)
        {
            TryInvokeIfFileAccessible(ConfigFile,
                () =>
                {
                    try
                    {
                        string json = Serialize(_favorites);
                        _fileSystem.File.WriteAllText(ConfigFile, json);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Failed to save favorites: {ex.Message}");
                    }
                });
        }
    }

    private void TryInvokeIfFileAccessible(string filePath, Action callBack, int maxAttempts = 5, int delay = 100)
    {
        int attempt = 0;

        while (attempt < maxAttempts)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath) && _fileSystem.File.Exists(filePath))
                {
                    using FileSystemStream? fs = _fileSystem.File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    fs.Close();
                }

                ThreadHelper.JoinableTaskFactory.Run(async delegate
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    callBack.Invoke();
                });

                return;
            }
            catch (IOException)
            {
                // File is locked, increment attempt count and wait before retrying
                attempt++;

                if (attempt < maxAttempts)
                {
                    Thread.Sleep(delay);
                }
                else
                {
                    throw;
                }
            }
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
}

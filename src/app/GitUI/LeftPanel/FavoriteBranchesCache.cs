#nullable enable

using System.Diagnostics;
using System.IO.Abstractions;
using System.Text.Json;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitUI.LeftPanel;

internal sealed class FavoriteBranchesCache
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true, Converters = { new ObjectIdConverter() } };

    private readonly IFileSystem _fileSystem;

    private readonly HashSet<BranchIdentifier> _favorites = [];
    private readonly object _lock = new();

    private bool _isLoaded;
    private string _location = string.Empty;
    private bool _isFeatureEnabled = false; // Flag to check if the feature is enabled

    public FavoriteBranchesCache(IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    }

    internal string ConfigFile
    {
        get
        {
            string? path = _fileSystem.Path.Combine(Location, "GitExtensions.favorite");

            if (_fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
            {
                _isFeatureEnabled = true;
                return path;
            }

            Trace.WriteLine($"Invalid or not accessible directory: {Location}. Please set a valid location.");
            _isFeatureEnabled = false; // Disable the feature if the directory is invalid

            return string.Empty;
        }
    }

    /// <summary>
    /// Gets or sets the location where the favorites configuration file is stored.
    /// </summary>
    internal string Location
    {
        get => _location;
        set
        {
            if (!string.Equals(_location, value, StringComparison.Ordinal))
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
    public void Remove(ObjectId? objectId, string branchName)
    {
        if (!_isFeatureEnabled || objectId == null || string.IsNullOrEmpty(branchName))
        {
            return;
        }

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

    public IEnumerable<IGitRef> Synchronize(IReadOnlyList<IGitRef> gitRefs, out IList<BranchIdentifier> noMatches)
    {
        if (!_isFeatureEnabled)
        {
            noMatches = new List<BranchIdentifier>(); // Return an empty list when feature is disabled

            return Enumerable.Empty<IGitRef>(); // Explicitly return empty if the feature is disabled
        }

        HashSet<IGitRef> matches = [];
        noMatches = [];

        lock (_lock)
        {
            LoadIfNeeded();
            IGitModule gitModule = gitRefs[0].Module;

            foreach (BranchIdentifier? favorite in _favorites)
            {
                IGitRef? exactMatch = gitRefs.FirstOrDefault(b => b.ObjectId == favorite.ObjectId && b.Name == favorite.Name);

                if (exactMatch is not null)
                {
                    matches.Add(exactMatch);

                    continue;
                }

                IGitRef? nameMatch = gitRefs.FirstOrDefault(b => b.Name == favorite.Name);

                if (nameMatch is not null)
                {
                    favorite.ObjectId = nameMatch.ObjectId;
                    matches.Add(nameMatch);

                    continue;
                }

                IGitRef? objectIdMatch = gitRefs.FirstOrDefault(b => b.ObjectId == favorite.ObjectId);

                if (objectIdMatch is not null)
                {
                    if (favorite.ObjectId != null)
                    {
                        ObjectId? latestCommitId = GetLatestCommitId(gitModule, favorite.ObjectId.ToString());

                        if (latestCommitId != null)
                        {
                            // favorite.Name = latestCommitId;
                            matches.Add(objectIdMatch);

                            continue;
                        }
                    }
                }

                noMatches.Add(favorite);
            }

            return matches;
        }
    }

    /// <summary>
    /// Adds a branch to the favorites list.
    /// </summary>
    /// <param name="objectId">The unique ObjectId of the branch's latest commit.</param>
    /// <param name="branchName">The name of the branch (fully qualified).</param>
    internal void Add(ObjectId objectId, string branchName)
    {
        if (!_isFeatureEnabled)
        {
            return; // Explicitly ignore if the feature is disabled
        }

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
        if (!_isFeatureEnabled)
        {
            return false; // Explicitly return false if the feature is disabled
        }

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
    /// Loads the list of favorite branches from the configuration file into memory.
    /// </summary>
    /// <remarks>
    /// This method reads the configuration file in JSON format and deserializes it into a list of `BranchIdentifier`
    /// objects. If the file does not exist, the method exits without taking any action.
    /// </remarks>
    internal void Load()
    {
        if (!_isFeatureEnabled || !_fileSystem.File.Exists(ConfigFile))
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
                        string? json = null;
                        json = _fileSystem.File.ReadAllText(ConfigFile);

                        if (string.IsNullOrEmpty(json))
                        {
                            return;
                        }

                        BranchIdentifier[]? deserialized = JsonSerializer.Deserialize<BranchIdentifier[]>(json, _jsonOptions);

                        if (deserialized is not null)
                        {
                            _favorites.Clear();
                            _favorites.UnionWith(deserialized);
                        }

                        _isLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Failed to load favorites: {ex.Message}");
                    }
                });
        }
    }

    internal void Save()
    {
        if (!_isFeatureEnabled)
        {
            return; // Explicitly ignore saving if the feature is disabled
        }

        lock (_lock)
        {
            TryInvokeIfFileAccessible(ConfigFile,
                () =>
                {
                    try
                    {
                        string json = JsonSerializer.Serialize(_favorites, _jsonOptions);
                        _fileSystem.File.WriteAllText(ConfigFile, json);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Failed to save favorites: {ex.Message}");
                    }
                });
        }
    }

    internal ObjectId? GetLatestCommitId(IGitModule gitModule, string nameMatchName)
    {
        if (!_isFeatureEnabled)
        {
            return null; // Explicitly return null if the feature is disabled
        }

        // Use the RevParse method to get the latest commit ID for the branch name
        return gitModule.RevParse(nameMatchName);
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

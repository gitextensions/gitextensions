using System.Text.Json;

namespace GitUI.LeftPanel
{
    public class CachedFavorites
    {
        private readonly HashSet<string> _favorites = new();
        private readonly object _lock = new();
        private bool _isLoaded;

        public IReadOnlyCollection<string> Favorites => _favorites;

        public string Location { get; set; } = string.Empty;

        private string ConfigFile
        {
            get => Path.Combine(Location, "favorites");
        }

        public void Add(string branch)
        {
            if (string.IsNullOrEmpty(branch))
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

        public void Remove(string branch)
        {
            if (string.IsNullOrEmpty(branch))
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

        public bool Contains(string branch)
        {
            if (string.IsNullOrEmpty(branch))
            {
                return false;
            }

            LoadIfNeeded();

            lock (_lock)
            {
                return _favorites.Contains(branch);
            }
        }

        private void Load()
        {
            if (_isLoaded || !File.Exists(ConfigFile))
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

                string[]? deserialized = JsonSerializer.Deserialize<string[]>(json);

                if (deserialized != null)
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
                    string json = JsonSerializer.Serialize(_favorites, new JsonSerializerOptions { WriteIndented = true });
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
            if (!_isLoaded)
            {
                Load();
            }
        }
    }
}

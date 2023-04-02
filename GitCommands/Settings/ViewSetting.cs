using System.Data.SqlTypes;

namespace GitCommands
{
    public abstract class ViewSetting<T> : IViewSetting
    {
        public T DefaultValue { get; init; }

        public string Name { get; init; }

        public abstract T Value { get; set; }

        public ViewSetting(string name, T defaultValue)
        {
            DefaultValue = defaultValue;
            Name = name;
        }

        /// <summary>
        /// Optionally calls <cref>Reload</cref> and returns the current value.
        /// </summary>
        public T GetValue(bool reload)
        {
            if (reload)
            {
                Reload();
            }

            return Value;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract void Reload();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void ResetToDefault() => Value = DefaultValue;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract void Save();
    }
}

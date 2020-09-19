#nullable enable

namespace GitUI.Shells
{
    public interface IHaveId<out T>
    {
        /// <summary>
        /// Gets a unique key.
        /// </summary>
        T Id { get; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using GitCommands.Git;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public interface IGitRevisionInfoProvider
    {
        /// <summary>
        /// Loads children item for the given <paramref name="item"/>.
        /// </summary>
        /// <returns>The item's children.</returns>
        IEnumerable<IGitItem> LoadChildren(IGitItem item);
    }

    public sealed class GitRevisionInfoProvider : IGitRevisionInfoProvider
    {
        private readonly Func<IGitModule> _getModule;

        public GitRevisionInfoProvider(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }

        /// <summary>
        /// Loads children item for the given <paramref name="item"/>.
        /// </summary>
        /// <returns>The item's children.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><see cref="IGitItem.Guid"/> is not supplied.</exception>
        public IEnumerable<IGitItem> LoadChildren(IGitItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (string.IsNullOrWhiteSpace(item.Guid))
            {
                throw new ArgumentException("Item must have a valid identifier", nameof(item.Guid));
            }

            var module = _getModule();

            if (module == null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
            }

            return YieldSubItems();

            IEnumerable<IGitItem> YieldSubItems()
            {
                var basePath = (item as GitItem)?.FileName ?? string.Empty;

                foreach (var subItem in module.GetTree(item.ObjectId, full: false))
                {
                    if (subItem is GitItem gitItem)
                    {
                        gitItem.FileName = Path.Combine(
                            basePath,
                            gitItem.FileName ?? string.Empty);
                    }

                    yield return subItem;
                }
            }
        }
    }
}
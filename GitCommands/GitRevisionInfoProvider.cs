using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public interface IGitRevisionInfoProvider
    {
        /// <summary>
        /// Loads children item for the given <paramref name="item"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The item's children.</returns>
        IEnumerable<IGitItem> LoadChildren(IGitItem item);
    }

    public sealed class GitRevisionInfoProvider : IGitRevisionInfoProvider
    {
        private readonly IGitModule _module;

        public GitRevisionInfoProvider(IGitModule module)
        {
            _module = module;
        }

        /// <summary>
        /// Loads children item for the given <paramref name="item"/>.
        /// </summary>
        /// <param name="item"></param>
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

            var subItems = _module.GetTree(item.Guid, false).ToList();
            foreach (var subItem in subItems.OfType<GitItem>())
            {
                subItem.FileName = Path.Combine((item as GitItem)?.FileName ?? string.Empty, subItem.FileName ?? string.Empty);
            }
            return subItems;
        }
    }
}
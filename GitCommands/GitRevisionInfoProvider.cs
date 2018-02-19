using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitCommands.Git;
using GitUIPluginInterfaces;
using System.Threading.Tasks;

namespace GitCommands
{
    public interface IGitRevisionInfoProvider
    {
        /// <summary>
        /// Loads children item for the given <paramref name="item"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The item's children.</returns>
        Task<IEnumerable<IGitItem>> LoadChildren(IGitItem item);
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
        /// <param name="item"></param>
        /// <returns>The item's children.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><see cref="IGitItem.Guid"/> is not supplied.</exception>
        public async Task<IEnumerable<IGitItem>> LoadChildren(IGitItem item)
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

            var subItems = (await module.GetTreeAsync(item.Guid, false)).ToList();
            foreach (var subItem in subItems.OfType<GitItem>())
            {
                subItem.FileName = Path.Combine((item as GitItem)?.FileName ?? string.Empty, subItem.FileName ?? string.Empty);
            }
            return subItems;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using GitCommands;

namespace GitUI
{
    public sealed class ArtificialCommitChangeCount
    {
        /// <summary>
        /// Number of changed files.
        /// </summary>
        public IReadOnlyList<GitItemStatus> Changed { get; private set; } = Array.Empty<GitItemStatus>();

        /// <summary>
        ///  Number of new files.
        /// </summary>
        public IReadOnlyList<GitItemStatus> New { get; private set; } = Array.Empty<GitItemStatus>();

        /// <summary>
        /// Number of deleted files.
        /// </summary>
        public IReadOnlyList<GitItemStatus> Deleted { get; private set; } = Array.Empty<GitItemStatus>();

        /// <summary>
        /// Number of submodules where the commit has changed
        /// (regardless if they are dirty or not).
        /// </summary>
        public IReadOnlyList<GitItemStatus> SubmodulesChanged { get; private set; } = Array.Empty<GitItemStatus>();

        /// <summary>
        /// Number of dirty submodules (with changes that are not committed)
        /// (regardless if the commit is changed or not).
        /// </summary>
        public IReadOnlyList<GitItemStatus> SubmodulesDirty { get; private set; } = Array.Empty<GitItemStatus>();

        /// <summary>
        /// Any change in any category.
        /// </summary>
        public bool HasChanges
            => (Changed?.Count ?? 0) > 0
               || (New?.Count ?? 0) > 0
               || (Deleted?.Count ?? 0) > 0
               || (SubmodulesChanged?.Count ?? 0) > 0
               || (SubmodulesDirty?.Count ?? 0) > 0;

        /// <summary>
        /// Update the change count.
        /// </summary>
        /// <param name="items">Git items.</param>
        public void Update(IReadOnlyList<GitItemStatus> items)
        {
            Changed = items.Where(item => !item.IsNew && !item.IsDeleted && !item.IsSubmodule).ToList();
            New = items.Where(item => item.IsNew && !item.IsSubmodule).ToList();
            Deleted = items.Where(item => item.IsDeleted && !item.IsSubmodule).ToList();
            SubmodulesChanged = items.Where(item => item.IsSubmodule && item.IsChanged).ToList();
            SubmodulesDirty = items.Where(item => item.IsSubmodule && item.IsDirty).ToList();
        }

        /// <summary>
        /// Summary for the changes, limited if too big.
        /// </summary>
        /// <returns>string with changes.</returns>
        public string GetSummary()
        {
            StringBuilder builder = new();

            // TODO use translation strings here
            Append(Changed, "changed file");
            Append(Deleted, "deleted file");
            Append(New, "new file");
            Append(SubmodulesChanged, "changed submodule");
            Append(SubmodulesDirty, "dirty submodule");

            return builder.ToString();

            void Append(IReadOnlyList<GitItemStatus> items, string singular)
            {
                if (items is null || items.Count == 0)
                {
                    return;
                }

                if (builder.Length != 0)
                {
                    builder.AppendLine();
                }

                builder.Append($"{items.Count} {singular}{(items.Count == 1 ? "" : "s")}")
                    .AppendLine();

                const int maxItems = 5;

                for (var i = 0; i < maxItems && i < items.Count; i++)
                {
                    builder.Append("- ").AppendLine(items[i].Name);
                }

                if (items.Count > maxItems)
                {
                    var unlistedCount = items.Count - maxItems;
                    builder.Append("- (").Append(unlistedCount).Append(" more file");
                    if (unlistedCount != 1)
                    {
                        builder.Append('s');
                    }

                    builder.Append(')').AppendLine();
                }
            }
        }
    }
}

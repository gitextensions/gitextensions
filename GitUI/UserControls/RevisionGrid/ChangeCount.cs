using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using GitCommands;

namespace GitUI
{
    public class ChangeCount
    {
        // Count for artificial commits
        public IReadOnlyList<GitItemStatus> Changed { get; set; }
        public IReadOnlyList<GitItemStatus> New { get; set; }
        public IReadOnlyList<GitItemStatus> Deleted { get; set; }
        public IReadOnlyList<GitItemStatus> SubmodulesChanged { get; set; }
        public IReadOnlyList<GitItemStatus> SubmodulesDirty { get; set; }

        public bool HasChanges
            => (Changed?.Count ?? 0) > 0
               || (New?.Count ?? 0) > 0
               || (Deleted?.Count ?? 0) > 0
               || (SubmodulesChanged?.Count ?? 0) > 0
               || (SubmodulesDirty?.Count ?? 0) > 0;

        public void UpdateChangeCount(IReadOnlyList<GitItemStatus> items)
        {
            Changed = items.Where(item => !item.IsNew && !item.IsDeleted && !item.IsSubmodule).ToList();
            New = items.Where(item => item.IsNew && !item.IsSubmodule).ToList();
            Deleted = items.Where(item => item.IsDeleted && !item.IsSubmodule).ToList();
            SubmodulesChanged = items.Where(item => item.IsSubmodule && item.IsChanged).ToList();
            SubmodulesDirty = items.Where(item => item.IsSubmodule && item.IsDirty).ToList();
        }

        public StringBuilder GetSummary()
        {
            StringBuilder builder = new StringBuilder();

            // TODO use translation strings here
            Append(Changed, "changed file");
            Append(Deleted, "deleted file");
            Append(New, "new file");
            Append(SubmodulesChanged, "changed submodule");
            Append(SubmodulesDirty, "dirty submodule");

            return builder;

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

                builder.Append(items.Count).Append(' ');

                if (items.Count == 1)
                {
                    builder.AppendLine(singular);
                }
                else
                {
                    builder.Append(singular).AppendLine("s");
                }

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

using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls
{
    public static class FileStatusItemExtensions
    {
        public static IEnumerable<GitRevision> FirstRevs(this IEnumerable<FileStatusItem> l)
        {
            return l.Where(i => i.FirstRevision != null)
                .Select(i => i.FirstRevision)
                .Distinct();
        }

        public static IEnumerable<ObjectId> FirstIds(this IEnumerable<FileStatusItem> l)
        {
            return l.Where(i => i.FirstRevision != null)
                .Select(i => i.FirstRevision.ObjectId)
                .Distinct();
        }

        public static IEnumerable<GitRevision> SecondRevs(this IEnumerable<FileStatusItem> l)
        {
            return l.Select(i => i.SecondRevision)
                .Distinct();
        }

        public static IEnumerable<ObjectId> SecondIds(this IEnumerable<FileStatusItem> l)
        {
            return l.Select(i => i.SecondRevision.ObjectId)
                .Distinct();
        }

        public static IEnumerable<GitItemStatus> Items(this IEnumerable<FileStatusItem> l)
        {
            return l.Select(i => i.Item);
        }
    }
}

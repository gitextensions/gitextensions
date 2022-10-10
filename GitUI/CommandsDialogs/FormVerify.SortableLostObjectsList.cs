namespace GitUI.CommandsDialogs
{
    partial class FormVerify
    {
        private sealed class SortableLostObjectsList : SortableBindingList<LostObject>
        {
            static SortableLostObjectsList()
            {
                AddSortableProperty(lostObject => lostObject.RawType, (x, y) => string.Compare(x.RawType, y.RawType, StringComparison.Ordinal));
                AddSortableProperty(lostObject => lostObject.ObjectId, (x, y) => x.ObjectId.CompareTo(y.ObjectId));
                AddSortableProperty(lostObject => lostObject.Author, (x, y) => string.Compare(x.Author, y.Author, StringComparison.CurrentCulture));
                AddSortableProperty(lostObject => lostObject.Date, (x, y) => x.Date.HasValue && y.Date.HasValue
                    ? DateTime.Compare(x.Date.Value, y.Date.Value)
                    : Comparer<bool>.Default.Compare(x.Date.HasValue, y.Date.HasValue));
                AddSortableProperty(lostObject => lostObject.Subject, (x, y) => string.Compare(x.Subject, y.Subject, StringComparison.CurrentCulture));
            }
        }
    }
}

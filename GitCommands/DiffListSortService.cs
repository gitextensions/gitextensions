using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCommands
{
    public class DiffListSortService : IDiffListSortService
    {
        private static readonly Lazy<DiffListSortService> _lazyDiffListSorting = new Lazy<DiffListSortService>(() => new DiffListSortService());
        public static DiffListSortService Instance => _lazyDiffListSorting.Value;
        private DiffListSortType _diffListSorting;
        public event EventHandler DiffListSortingChanged;

        public DiffListSortType DiffListSorting
        {
            get { return _diffListSorting; }
            set
            {
                var previous = _diffListSorting;
                if (previous != value)
                {
                    _diffListSorting = value;
                    AppSettings.DiffListSorting = value;
                    OnDiffListSortingChanged();
                }
            }
        }

        public DiffListSortService()
        {
            DiffListSorting = GetSettingValueOrDefault();
        }

        private DiffListSortType GetSettingValueOrDefault()
        {
            return AppSettings.DiffListSorting;
        }

        protected void OnDiffListSortingChanged()
        {
            DiffListSortingChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

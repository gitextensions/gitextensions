using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCommands
{
    public enum DiffListSortType
    {
        /// <summary>
        /// Sorts by file path alphanumerically
        /// </summary>
        FilePath,

        /// <summary>
        /// Sorts by file extension then by path
        /// </summary>
        FileExtension,

        /// <summary>
        /// Sorts by git change type. Addition, Deletions, edits, etc. then by path
        /// </summary>
        FileStatus
    }

    public interface IDiffListSortService
    {
        DiffListSortType DiffListSorting { get; set; }

        event EventHandler DiffListSortingChanged;
    }

    public static class DiffListSortServiceExtensions
    {
        /// <summary>
        /// Provides the <see cref="IDiffListSortService.DiffListSorting"/> immediately and then an element for each <see cref="IDiffListSortService.DiffListSortingChanged"/>.
        /// </summary>
        /// <param name="diffListSortService">The diff list service</param>
        /// <returns>A hot stream with one immediate cold element.</returns>
        public static IObservable<DiffListSortType> CurrentAndFutureSorting(this IDiffListSortService diffListSortService)
        {
            return Observable.Return(diffListSortService.DiffListSorting)
                .Concat(Observable.FromEventPattern(
                    h => diffListSortService.DiffListSortingChanged += h,
                    h => diffListSortService.DiffListSortingChanged -= h)
                    .Select(_ => diffListSortService.DiffListSorting))
                .DistinctUntilChanged();
        }
    }
}

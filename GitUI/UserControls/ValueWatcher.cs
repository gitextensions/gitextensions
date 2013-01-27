using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitUI.UserControls
{
    /// <summary>Base class for watching a variable's dirty status. 
    /// Value is retrieved async, while updates are run on the UI thread.</summary>
    internal abstract class ValueWatcher
    {
        /// <summary>Gets the new values on a background thread, and if necessary, updates on the UI thread.</summary>
        public abstract Task CheckUpdateAsync();
    }

    /// <summary>Watches a variable's dirty status. 
    /// Value is retrieved async, while updates are run on the UI thread.</summary>
    class ValueWatcher<T> : ValueWatcher
        where T : class
    {
        //public bool IsDirty { get; private set; }
        T _currentValue;
        readonly Action<T> _onChanged;
        readonly Func<T> _getValue;
        readonly Func<T, T, bool> _equals;

        public ValueWatcher(T initialValue, Func<T> getValue, Action<T> onChanged, Func<T, T, bool> equality)
        {
            _currentValue = initialValue;
            _onChanged = onChanged;
            _getValue = getValue;
            _equals = equality ?? Equals;
        }

        /// <summary>Passed between non-UI task and UI task.</summary>
        class DirtyResults<TValue>
            where TValue : class
        {
            /// <summary>Indicates whether the value is dirty/needs updating.</summary>
            public bool IsDirty;
            /// <summary>The new value to update with.</summary>
            public TValue NewValue;
        }

        /// <summary>Gets the new values on a background thread, and if necessary, updates on the UI thread.</summary>
        public override Task CheckUpdateAsync()
        {
            return
                Task.Factory
                    .StartNew(
                        () =>
                        {
                            T newValue = _getValue();
                            if (_currentValue == null && newValue == null)
                            {// both still null
                                return new DirtyResults<T> { IsDirty = false };
                            }

                            if ((_currentValue != null && newValue == null)
                                ||
                                (_currentValue == null && newValue != null)
                                ||
                                (_equals(_currentValue, newValue) == false)
                                )
                            {// XOR null/not null OR not equal -> update
                                return new DirtyResults<T> { IsDirty = true, NewValue = newValue };
                            }// ... else equal
                            return new DirtyResults<T> { IsDirty = false };
                        })
                    .ContinueWith(
                        dirtyResults =>
                        {
                            var results = dirtyResults.Result;
                            if (results.IsDirty)
                            {
                                Update(results.NewValue);
                            }
                        },
                        TaskScheduler.FromCurrentSynchronizationContext());
        }

        void Update(T newValue)
        {
            //IsDirty = true;
            _currentValue = newValue;
            _onChanged(_currentValue);
            //IsDirty = false;
        }
    }

    /// <summary>Watches a collection's dirty status. 
    /// Values are retrieved async, while updates are run on the UI thread.</summary>
    class ListWatcher<T> : ValueWatcher<ICollection<T>>
    {
        public ListWatcher(Func<ICollection<T>> getValues, Action<ICollection<T>> onChanged)
            : base(null, getValues, onChanged, (olds, news) => olds.SequenceEqual(news)) { }
    }
}

using System.Runtime.InteropServices;
using static System.NativeMethods;

namespace GitUI.Theming
{
    internal class SystemBrushesCache : IDisposable
    {
        private readonly Dictionary<int, HandleRef> _cache = new();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            foreach (int key in _cache.Keys)
            {
                DeleteObject(_cache[key].Handle);
            }

            _cache.Clear();
        }

        public IntPtr GetBrush(int colorref)
        {
            if (!_cache.TryGetValue(colorref, out var handle))
            {
                IntPtr hbrush = CreateSolidBrush(colorref);
                handle = new HandleRef(this, hbrush);

                _cache.Add(colorref, handle);
            }

            return handle.Handle;
        }
    }
}

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace GitUI.Shells
{
    public static class ResourceImagesProvider
    {
        public static readonly IReadOnlyDictionary<string, Bitmap> Images = typeof(Properties.Images)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(x => x.PropertyType == typeof(Bitmap))
            .ToDictionary(x => x.Name, x => (Bitmap)x.GetValue(null));
    }
}

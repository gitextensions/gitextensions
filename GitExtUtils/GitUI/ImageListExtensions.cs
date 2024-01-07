using System.Runtime.InteropServices;

namespace GitUI
{
    public static class ImageListExtensions
    {
        internal const int ComCtl32CLRNone = unchecked((int)0xFFFFFFFF);

        [DllImport("comctl32.dll", EntryPoint = "ImageList_SetBkColor")]
        internal static extern int ImageListSetBkColor(IntPtr himl, int clrBk);

        /// <summary>
        /// A regression was introduced in .net8 which causes an incorrect background color to be set
        /// which manifests when using transparent images. See https://github.com/dotnet/winforms/issues/10462
        /// This metod should be removed once the underlying issue is fixed.
        /// </summary>
        public static ImageList FixImageTransparencyRegression(this ImageList imageList)
        {
            ImageListSetBkColor(imageList.Handle, ComCtl32CLRNone);
            return imageList;
        }
    }
}

namespace GitUI;

public static class ImageListExtensions
{
    /// <summary>
    /// A regression was introduced in .NET 8 which causes an incorrect background color to be set
    /// which manifests when using transparent images. See https://github.com/dotnet/winforms/issues/10462
    /// This method should be removed once the underlying issue is fixed.
    /// </summary>
    public static ImageList FixImageTransparencyRegression(this ImageList imageList)
    {
        NativeMethods.ImageListSetBkColor(imageList.Handle, NativeMethods.ComCtl32CLRNone);
        return imageList;
    }
}

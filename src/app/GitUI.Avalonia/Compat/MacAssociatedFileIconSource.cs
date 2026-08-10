using System.Runtime.InteropServices;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace GitUI.Compat;

internal sealed class MacAssociatedFileIconSource : IAssociatedFileIconSource
{
    private const long NsBitmapImageFileTypePng = 4;

    public IImage? Get(string workingDirectory, string relativeFilePath)
    {
        IntPtr pool = IntPtr.Zero;
        try
        {
            pool = Send(GetClass("NSAutoreleasePool"), "new");
            IntPtr workspace = Send(GetClass("NSWorkspace"), "sharedWorkspace");
            IntPtr extension = CreateString(Path.GetExtension(relativeFilePath).TrimStart('.'));
            IntPtr image = Send(workspace, "iconForFileType:", extension);
            IntPtr tiffData = Send(image, "TIFFRepresentation");
            IntPtr representation = Send(GetClass("NSBitmapImageRep"), "imageRepWithData:", tiffData);
            IntPtr properties = Send(GetClass("NSDictionary"), "dictionary");
            IntPtr pngData = Send(representation, "representationUsingType:properties:", NsBitmapImageFileTypePng, properties);
            if (pngData == IntPtr.Zero)
            {
                return null;
            }

            nuint length = SendUnsigned(pngData, "length");
            IntPtr bytes = Send(pngData, "bytes");
            if (length == 0 || length > int.MaxValue || bytes == IntPtr.Zero)
            {
                return null;
            }

            byte[] encoded = new byte[(int)length];
            Marshal.Copy(bytes, encoded, 0, encoded.Length);
            using MemoryStream stream = new(encoded, writable: false);
            return new Bitmap(stream);
        }
        catch (DllNotFoundException)
        {
            return null;
        }
        catch (EntryPointNotFoundException)
        {
            return null;
        }
        catch (Exception exception) when (exception is IOException or ArgumentException)
        {
            return null;
        }
        finally
        {
            if (pool != IntPtr.Zero)
            {
                Send(pool, "drain");
            }
        }
    }

    private static IntPtr CreateString(string value)
    {
        IntPtr utf8 = Marshal.StringToCoTaskMemUTF8(value);
        try
        {
            return ObjcMsgSend(GetClass("NSString"), GetSelector("stringWithUTF8String:"), utf8);
        }
        finally
        {
            Marshal.FreeCoTaskMem(utf8);
        }
    }

    private static IntPtr GetClass(string name) => ObjcGetClass(name);

    private static IntPtr GetSelector(string name) => SelRegisterName(name);

    private static IntPtr Send(IntPtr receiver, string selector)
        => ObjcMsgSend(receiver, GetSelector(selector));

    private static IntPtr Send(IntPtr receiver, string selector, IntPtr argument)
        => ObjcMsgSend(receiver, GetSelector(selector), argument);

    private static IntPtr Send(IntPtr receiver, string selector, long firstArgument, IntPtr secondArgument)
        => ObjcMsgSend(receiver, GetSelector(selector), firstArgument, secondArgument);

    private static nuint SendUnsigned(IntPtr receiver, string selector)
        => ObjcMsgSendUnsigned(receiver, GetSelector(selector));

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
    private static extern IntPtr ObjcGetClass(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr SelRegisterName(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr ObjcMsgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr ObjcMsgSend(IntPtr receiver, IntPtr selector, IntPtr argument);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr ObjcMsgSend(IntPtr receiver, IntPtr selector, long firstArgument, IntPtr secondArgument);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern nuint ObjcMsgSendUnsigned(IntPtr receiver, IntPtr selector);
}

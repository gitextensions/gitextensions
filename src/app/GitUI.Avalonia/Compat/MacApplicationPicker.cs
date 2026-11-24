using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace GitUI.Compat;

// Framework constraint: Avalonia12.1 filters selected application bundles out as directories.
internal static partial class MacApplicationPicker
{
    private const string ObjectiveC = "/usr/lib/libobjc.A.dylib";

    // parity-scaffolding: substitute the external AppKit selection, not the caller's inputs.
    private static Func<TopLevel, string?> _showPanel = ShowPanel;

    internal static string? Pick(TopLevel owner) => _showPanel(owner);

    private static string? ShowPanel(TopLevel owner)
    {
        owner.Dispatcher.VerifyAccess();
        nint pool = Send(GetClass("NSAutoreleasePool"), GetSelector("new"));
        try
        {
            return ShowPanelCore(owner);
        }
        finally
        {
            if (pool != 0)
            {
                Send(pool, GetSelector("drain"));
            }
        }
    }

    private static string? ShowPanelCore(TopLevel owner)
    {
        nint panel = Send(GetClass("NSOpenPanel"), GetSelector("openPanel"));
        if (panel == 0)
        {
            throw new InvalidOperationException("The macOS application chooser is unavailable.");
        }

        nint extension = SendUtf8(GetClass("NSString"), GetSelector("stringWithUTF8String:"), "app");
        nint types = SendObject(GetClass("NSArray"), GetSelector("arrayWithObject:"), extension);
        SetObject(panel, GetSelector("setAllowedFileTypes:"), types);
        SendBoolean(panel, GetSelector("setAllowsOtherFileTypes:"), 0);
        SendBoolean(panel, GetSelector("setCanChooseFiles:"), 1);
        SendBoolean(panel, GetSelector("setCanChooseDirectories:"), 0);
        SendBoolean(panel, GetSelector("setTreatsFilePackagesAsDirectories:"), 0);
        SendBoolean(panel, GetSelector("setAllowsMultipleSelection:"), 0);

        nint parent = owner.TryGetPlatformHandle() is { HandleDescriptor: "NSWindow" } handle ? handle.Handle : 0;
        if (parent != 0)
        {
            SetTwoObjects(parent, GetSelector("addChildWindow:ordered:"), panel, 1);
        }

        try
        {
            if (Send(panel, GetSelector("runModal")) != 1)
            {
                return null;
            }

            nint urls = Send(panel, GetSelector("URLs"));
            nint url = Send(urls, GetSelector("firstObject"));
            nint path = Send(url, GetSelector("path"));
            return Marshal.PtrToStringUTF8(Send(path, GetSelector("UTF8String")))
                ?? throw new IOException("The application chooser returned no path.");
        }
        finally
        {
            SetObject(panel, GetSelector("orderOut:"), 0);
            if (parent != 0)
            {
                SetObject(parent, GetSelector("removeChildWindow:"), panel);
            }
        }
    }

    [LibraryImport(ObjectiveC, EntryPoint = "objc_getClass", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint GetClass(string name);

    [LibraryImport(ObjectiveC, EntryPoint = "sel_registerName", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint GetSelector(string name);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static partial nint Send(nint receiver, nint selector);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static partial nint SendObject(nint receiver, nint selector, nint value);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static partial void SetObject(nint receiver, nint selector, nint value);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static partial void SetTwoObjects(nint receiver, nint selector, nint value, nint secondValue);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static partial void SendBoolean(nint receiver, nint selector, byte value);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint SendUtf8(nint receiver, nint selector, string value);

    // parity-scaffolding: external chooser responses are injectable without manufacturing a selected app in the UI.
    internal static class TestAccessor
    {
        public static Func<TopLevel, string?> ShowPanel
        {
            get => _showPanel;
            set => _showPanel = value;
        }
    }
}

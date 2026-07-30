using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Xml.Serialization;
using Avalonia.Controls;
using GitCommands;

namespace GitUI;

/// <summary>
///   Stores the state and position of a single window.
/// </summary>
[DebuggerDisplay("Name={Name} Rect={Rect} DeviceDpi={DeviceDpi} State={State}")]
public class WindowPosition
{
    protected WindowPosition()
    {
        DeviceDpi = 96;
    }

    public WindowPosition(Rectangle rect, int deviceDpi, WindowState state, string name)
    {
        Rect = rect;
        DeviceDpi = deviceDpi;
        State = state;
        Name = name;
    }

    public Rectangle Rect { get; set; }

    [DefaultValue(96)]
    public int DeviceDpi { get; set; }

    public WindowState State { get; set; }

    public string? Name { get; set; }
}

public class WindowPositionList
{
    private static readonly XmlSerializer _serializer = new(typeof(WindowPositionList));

    // parity-scaffolding: Lets the headless parity/test process isolate geometry from the user's settings.
    internal static Func<string> ConfigFilePathProvider { get; set; }
        = () => Path.Join(AppSettings.LocalApplicationDataPath.Value!, "WindowPositions.xml");

    private static string ConfigFilePath => ConfigFilePathProvider();

    public List<WindowPosition> WindowPositions { get; set; } = [];

    protected WindowPositionList()
    {
    }

    public WindowPosition? Get(string name)
    {
        return WindowPositions.FirstOrDefault(r => r.Name == name);
    }

    public void AddOrUpdate(WindowPosition pos)
    {
        WindowPositions.RemoveAll(r => r.Name == pos.Name);
        WindowPositions.Add(pos);
    }

    public static WindowPositionList? Load()
    {
        if (!File.Exists(ConfigFilePath))
        {
            return new WindowPositionList();
        }

        try
        {
            using FileStream stream = File.Open(ConfigFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
            return (WindowPositionList?)_serializer.Deserialize(stream);
        }
        catch
        {
            return new WindowPositionList();
        }
    }

    public void Save()
    {
        // Cross-platform: the XDG application-data directory may not exist before the first window closes.
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath)!);
        using FileStream stream = File.Open(ConfigFilePath, FileMode.Create, FileAccess.Write);
        _serializer.Serialize(stream, this);
    }
}

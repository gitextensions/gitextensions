using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace GitUI.Compat;

/// <summary>
/// Records recently used items in the freedesktop.org desktop bookmark file (<c>recently-used.xbel</c>),
/// which GLib, GTK and KDE's task manager read.
/// </summary>
internal static class RecentlyUsedBookmarks
{
    private const string MetadataOwner = "http://freedesktop.org";
    private const string DirectoryMimeType = "inode/directory";
    private static readonly XNamespace BookmarkNamespace = "http://www.freedesktop.org/standards/desktop-bookmarks";
    private static readonly XNamespace MimeNamespace = "http://www.freedesktop.org/standards/shared-mime-info";

    /// <summary>
    /// Adds or refreshes the bookmark of a directory and the application that used it, keeping every
    /// other entry of the file.
    /// </summary>
    /// <param name="file">The bookmark file; it is created when missing.</param>
    /// <param name="directory">The used directory.</param>
    /// <param name="title">The name shown for the bookmark.</param>
    /// <param name="applicationName">The application, named like its desktop file without the extension.</param>
    /// <param name="command">The command that opens the directory, with <c>%f</c> for its path.</param>
    /// <param name="now">The time of use.</param>
    /// <exception cref="XmlException">The existing file is not a bookmark file; it is left unchanged.</exception>
    internal static void AddDirectory(string file, string directory, string title, string applicationName, string command, DateTime now)
    {
        XDocument document = File.Exists(file)
            ? XDocument.Load(file)
            : new XDocument(new XElement(
                "xbel",
                new XAttribute("version", "1.0"),
                new XAttribute(XNamespace.Xmlns + "bookmark", BookmarkNamespace.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "mime", MimeNamespace.NamespaceName)));
        XElement root = document.Root is { Name.LocalName: "xbel" } xbel
            ? xbel
            : throw new XmlException($"'{file}' is not a desktop bookmark file.");

        string href = new Uri(Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory))).AbsoluteUri;
        string stamp = now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'", CultureInfo.InvariantCulture);

        XElement? bookmark = root.Elements("bookmark").FirstOrDefault(element => (string?)element.Attribute("href") == href);
        if (bookmark is null)
        {
            bookmark = new XElement("bookmark", new XAttribute("href", href), new XAttribute("added", stamp));
            root.Add(bookmark);
        }

        bookmark.SetAttributeValue("modified", stamp);
        bookmark.SetAttributeValue("visited", stamp);

        // The bookmark format lists the title before the metadata.
        if (bookmark.Element("title") is { } titleElement)
        {
            titleElement.Value = title;
        }
        else
        {
            bookmark.AddFirst(new XElement("title", title));
        }

        XElement info = GetOrAdd(bookmark, "info");
        XElement metadata = info.Elements("metadata").FirstOrDefault(element => (string?)element.Attribute("owner") == MetadataOwner)
            ?? Add(info, new XElement("metadata", new XAttribute("owner", MetadataOwner)));

        XElement mimeType = GetOrAdd(metadata, MimeNamespace + "mime-type");
        mimeType.SetAttributeValue("type", DirectoryMimeType);

        XElement applications = GetOrAdd(metadata, BookmarkNamespace + "applications");
        XElement application = applications.Elements(BookmarkNamespace + "application")
            .FirstOrDefault(element => (string?)element.Attribute("name") == applicationName)
            ?? Add(applications, new XElement(BookmarkNamespace + "application", new XAttribute("name", applicationName)));
        int count = int.TryParse((string?)application.Attribute("count"), NumberStyles.None, CultureInfo.InvariantCulture, out int previous) ? previous : 0;
        application.SetAttributeValue("exec", $"'{command}'");
        application.SetAttributeValue("modified", stamp);
        application.SetAttributeValue("count", (count + 1).ToString(CultureInfo.InvariantCulture));

        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        string temporary = $"{file}.{Environment.ProcessId}.tmp";
        try
        {
            using (XmlWriter writer = XmlWriter.Create(temporary, new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = true }))
            {
                document.Save(writer);
            }

            // Readers either see the previous file or the complete new one.
            File.Move(temporary, file, overwrite: true);
        }
        finally
        {
            File.Delete(temporary);
        }
    }

    private static XElement GetOrAdd(XElement parent, XName name) => parent.Element(name) ?? Add(parent, new XElement(name));

    private static XElement Add(XElement parent, XElement child)
    {
        parent.Add(child);
        return child;
    }
}
